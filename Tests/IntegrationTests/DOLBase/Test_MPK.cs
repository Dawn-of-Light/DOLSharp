using DOL.MPK;
using NUnit.Framework;
using System.Text;
using System.IO;

namespace DOL.Integration.DOLBase
{
    [TestFixture]
    class Test_MPK
    {
        private string testDirectory => "testMPK";
        private string textFileLocation => "test.txt";
        private string mpkFileLocation => "test.mpk";
        private string extractPath => "extractDirectory";
        private string textFileContent => "1234";

        [OneTimeSetUp]
        public void CreateFilesToBeAddedToMPK()
        {
            Directory.CreateDirectory(testDirectory);
            Directory.SetCurrentDirectory(testDirectory);

            var testFile = File.Create(textFileLocation);
            var fileContent = Encoding.ASCII.GetBytes(textFileContent);
            testFile.Write(fileContent, 0, fileContent.Length);
            testFile.Close();
        }

        [Test, Order(1)]
        public void Save_TestTxtToMPK_CorrectCRC()
        {
            var newMPK = new MPK.MPK(mpkFileLocation, true);
            var mpkFile = new MPKFile(textFileLocation);
            newMPK.AddFile(mpkFile);
            newMPK[textFileLocation].Header.TimeStamp = 0; //Make MPK creation deterministic

            newMPK.Save();
            Assert.That(newMPK.Count, Is.EqualTo(1));

            var expectedCRCValue = 375344986;
            Assert.That(newMPK.CRCValue, Is.EqualTo(expectedCRCValue));
        }

        [Test, Order(2)]
        public void Open_TestMPK_NoExceptions()
        {
            _ = new MPK.MPK(mpkFileLocation, false);
        }

        [Test, Order(3)]
        public void Extract_TestMPK_SameTxtContent()
        {
            var mpk = new MPK.MPK(mpkFileLocation, false);
            mpk.Extract(extractPath);

            var actualFileText = File.ReadAllText(Path.Combine(extractPath,  textFileLocation));
            var expectedFileText = textFileContent;
            Assert.That(actualFileText, Is.EqualTo(expectedFileText));
        }

        [OneTimeTearDown]
        public void RemoveArtifacts()
        {
            Directory.SetCurrentDirectory("..");
            Directory.Delete(Path.Combine(testDirectory), true);
        }
    }
}
