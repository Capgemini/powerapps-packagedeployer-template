using Capgemini.Xrm.Deployment.Core.Exceptions;
using Capgemini.Xrm.Deployment.Repository;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Xrm.Sdk;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Capgemini.Xrm.Deployment.DocTemplates
{
    public class DocTemplateManager
    {
        #region Constructors and private fields

        private readonly ICrmTemplatesRepository _templRepo;

        public DocTemplateManager(ICrmTemplatesRepository templRepo)
        {
            this._templRepo = templRepo;
        }

        #endregion Constructors and private fields

        #region Public class implementation

        public void SaveTemplateToFile(string templateName, string filePath)
        {
            var template = this._templRepo.GetDocumentTemplateByName(templateName);
            if (template == null)
                throw new ValidationException("Cannot find template " + templateName);

            string documentContent = template.GetAttributeValue<string>("content");
            string name = template.GetAttributeValue<string>("name");
            byte[] content = Convert.FromBase64String(documentContent);

            File.WriteAllBytes(filePath, content);
        }

        public void ImportTemplateFromFile(string templateName, string filePath)
        {
            var file = new FileInfo(filePath);
            OptionSetValue templType = new OptionSetValue(file.Extension.ToUpper() == "xlsx" ? 1 : 2);

            if (templType.Value != 2)
                throw new ValidationException("Only docx word templates are supported! (documenttype 2)");

            byte[] content = File.ReadAllBytes(filePath);

            using (MemoryStream stream = new MemoryStream(content))
            {
                string entityName = ReplaceCodeInWorld(stream);
                byte[] contentChanged = stream.ToArray();

                this._templRepo.SetTemplate(templateName, entityName, templType, contentChanged);
            }
        }

        #endregion Public class implementation

        #region Private class implementation

        private string ReplaceCodeInWorld(Stream templateStream)
        {
            string entityName = "";

            using (var doc = WordprocessingDocument.Open(templateStream, true, new OpenSettings { AutoSave = true }))
            {
                Tuple<string, string> entAndCode = GetEntityAndCode(doc.MainDocumentPart.Document.InnerXml);
                string replaceWith = entAndCode.Item1 + "/" + this._templRepo.CurrentAccess.GetEntityCode(entAndCode.Item1);
                string toFind = entAndCode.Item1 + "/" + entAndCode.Item2;

                // crm keeps the etc in multiple places; parts here are the actual merge fields
                doc.MainDocumentPart.Document.InnerXml = doc.MainDocumentPart.Document.InnerXml.Replace(toFind, replaceWith);

                // next is the actual namespace declaration
                doc.MainDocumentPart.CustomXmlParts.ToList().ForEach(a =>
                {
                    System.Xml.XmlDocument customPart = new System.Xml.XmlDocument();
                    customPart.Load(a.GetStream());

                    if (customPart.InnerXml.IndexOf(toFind, StringComparison.Ordinal) > -1)
                    {
                        customPart.InnerXml = customPart.InnerXml.Replace(toFind, replaceWith);
                        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(customPart.OuterXml)))
                        {
                            a.FeedData(stream);
                        }
                    }
                });

                entityName = entAndCode.Item1;
            }

            return entityName;
        }

        private Tuple<string, string> GetEntityAndCode(string text)
        {
            string regExpr = @"urn:microsoft-crm\/document-template\/\w*\/\d+\/";

            var matches = Regex.Matches(text, regExpr);

            foreach (Match item in matches)
            {
                string entityAndcode = item.Value.Replace("urn:microsoft-crm/document-template/", "");
                string[] bits = entityAndcode.Split('/');
                return new Tuple<string, string>(bits[0], bits[1]);
            }

            throw new ValidationException("Cannot find Entity and Code in template body");
        }

        #endregion Private class implementation
    }
}