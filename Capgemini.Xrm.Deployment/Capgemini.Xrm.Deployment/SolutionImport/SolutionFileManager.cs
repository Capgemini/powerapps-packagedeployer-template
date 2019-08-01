using Capgemini.Xrm.Deployment.Core.Model;
using Ionic.Zip;
using System.IO;
using System.Xml;

namespace Capgemini.Xrm.Deployment.SolutionImport
{
    public class SolutionFileManager
    {
        #region Private Fields

        private const string HoldingSolutionSuffix = "_Upgrade";
        private SolutionDetails _details;
        private readonly FileInfo _solutionFile;
        private readonly bool _forceUpgrade;

        #endregion Private Fields

        #region Constructors

        public SolutionFileManager(string solutionPath, bool forceUpgrade)
        {
            _solutionFile = new FileInfo(solutionPath);
            _forceUpgrade = forceUpgrade;
        }

        #endregion Constructors

        #region Public Properties and Methods

        public SolutionDetails SolutionDetails
        {
            get
            {
                if (_details == null)
                {
                    string holdSolName = _solutionFile.Name.Replace(_solutionFile.Extension, "") + HoldingSolutionSuffix;
                    string holdFileName = holdSolName + _solutionFile.Extension;
                    var holdFullPath = Path.Combine(_solutionFile.DirectoryName, holdFileName);

                    var solFilePath = Path.Combine(_solutionFile.DirectoryName, "solution.xml");

                    using (ZipFile zipFile = ZipFile.Read(_solutionFile.FullName))
                    {
                        foreach (ZipEntry e in zipFile)
                        {
                            if (e.FileName == "solution.xml")
                            {
                                e.Extract(_solutionFile.DirectoryName, ExtractExistingFileAction.OverwriteSilently);
                                break;
                            }
                        }
                    }

                    _details = ReadDetailsFromSolutionFile(solFilePath);
                    File.Delete(solFilePath);

                    _details.HoldingSolutionName = _details.SolutionName + HoldingSolutionSuffix;
                    _details.SolutionFilePath = _solutionFile.FullName;
                    _details.HoldingSolutionFilePath = holdFullPath;
                }

                return _details;
            }
        }

        public void CreateHoldingSolutionFile()
        {
            SolutionDetails details = SolutionDetails;

            File.Copy(details.SolutionFilePath, details.HoldingSolutionFilePath, true);

            var solFilePath = Path.Combine(_solutionFile.DirectoryName, "solution.xml");

            using (ZipFile zipFile = ZipFile.Read(details.HoldingSolutionFilePath))
            {
                foreach (ZipEntry e in zipFile)
                {
                    if (e.FileName == "solution.xml")
                    {
                        e.Extract(_solutionFile.DirectoryName, ExtractExistingFileAction.OverwriteSilently);
                        break;
                    }
                }

                var allText = File.ReadAllText(solFilePath);
                allText = allText.Replace("<UniqueName>" + _details.SolutionName + "</UniqueName>", "<UniqueName>" + _details.SolutionName + HoldingSolutionSuffix + "</UniqueName>");

                File.Delete(solFilePath);
                File.WriteAllText(solFilePath, allText);

                zipFile.UpdateFile(solFilePath, "");
                zipFile.Save();
            }

            File.Delete(solFilePath);
        }

        public void DeleteHoldingSolutionFile()
        {
            SolutionDetails details = SolutionDetails;

            if (File.Exists(details.HoldingSolutionFilePath))
            {
                File.Delete(details.HoldingSolutionFilePath);
            }
        }

        #endregion Public Properties and Methods

        #region Internall class implementation

        private SolutionDetails ReadDetailsFromSolutionFile(string solFilePath)
        {
            var doc = new XmlDocument();
            doc.Load(solFilePath);

            var outObj = new SolutionDetails
            {
                SolutionVersion = new System.Version(doc.SelectSingleNode("ImportExportXml/SolutionManifest/Version").InnerText),
                SolutionName = doc.SelectSingleNode("ImportExportXml/SolutionManifest/UniqueName").InnerText,
                ForceUpdate = _forceUpgrade
            };

            return outObj;
        }

        #endregion Internall class implementation
    }
}