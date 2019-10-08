﻿/***************************************
//   Copyright 2014 - Svetoslav Vasilev

//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at

//     http://www.apache.org/licenses/LICENSE-2.0

//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
*****************************************/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Text.RegularExpressions;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using TransMock.Mockifier.Parser;

using Moq;

namespace TransMock.Mockifier.Parser.Tests
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class TestBindingsParser
    {
        
        private static Dictionary<string, string> expectedTransportConfig;

        private static Dictionary<string, string> expectedCustomURLs;

        public TestBindingsParser()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize]
        public static void TestSuiteInitialize(TestContext testContext) 
        {
            if (expectedTransportConfig == null)
                expectedTransportConfig = new Dictionary<string, string>(6);

            if (expectedTransportConfig.Count == 0)
            {
                expectedTransportConfig.Add("BTS2010_ReceiveLocationOneWay",
                     CreateBTS2010OneWayReceiveLocationConfig());
    
                expectedTransportConfig.Add("BTS2010_ReceiveLocationTwoWays",
                    CreateBTS2010TwoWayReceiveLocationConfig());

                expectedTransportConfig.Add("BTS2010_SendPortOneWay",
                    CreateBTS2010OneWaySendPortConfig());

                expectedTransportConfig.Add("BTS2010_SendPortTwoWays",
                    CreateBTS2010TwoWaySendPortConfig());

                expectedTransportConfig.Add("BTS2013_ReceiveLocationOneWay",
                    CreateBTS2013OneWayReceiveLocation());

                expectedTransportConfig.Add("BTS2013_ReceiveLocationTwoWays",
                    CreateBTS2013TwoWayReceiveLocationConfig());

                expectedTransportConfig.Add("BTS2013_SendPortOneWay",
                    CreateBTS2013OneWaySendPortConfig());
                                
                expectedTransportConfig.Add("BTS2013_SendPortTwoWays",
                    CreateBTS2013TwoWaySendPortConfig());
            }

            if (expectedCustomURLs == null)
            {
                expectedCustomURLs = new Dictionary<string, string>(4);

                expectedCustomURLs.Add("OneWaySendFILE", "OneWayOutEndpoint");
                expectedCustomURLs.Add("TwoWayTestSendWCF", "TwoWayOutEndpoint/UpdateState");
                expectedCustomURLs.Add("OneWayReceive_FILE", "OneWayInEndpoint/StateChanged");
                expectedCustomURLs.Add("TwoWayTestReceive_WCF", "TwoWayInEndpoint");
            }
        }        
        
        // Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void TestSuiteCleanup() 
        {
            if (expectedTransportConfig != null)
            {
                expectedTransportConfig.Clear();               
            }

            if (expectedCustomURLs != null)
            {
                expectedCustomURLs.Clear();                
            }
        }
        
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
         //Use TestCleanup to run code after each test has run
         [TestCleanup()]
         public void TestCleanup() 
         {
             if (System.IO.File.Exists("TestApplicationMockAddresses.cs"))
             {
                 System.IO.File.Delete("TestApplicationMockAddresses.cs");
             }
         }
        
        #endregion

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddresses.cs.txt")]
        public void TestInlineParsing_SimpleMock_BTS2010()
        {            
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfo.xml", "TestApplication.BindingInfo_parsed.xml", null, "2010");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2010");               
                
            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2010");

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddresses.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }
            
        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddresses.cs.txt")]
        public void TestInlineParsing_SimpleMock_Unescape_BTS2010()
        {
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfo.xml", 
                "TestApplication.BindingInfo_parsed.xml", "2010", true);

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2010", true);

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2010", null, true);

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddresses.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }

        } 

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddresses.cs.txt")]
        public void TestInlineParsing_SimpleMock_ClassOutputSpecified_BTS2010()
        {            
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfo.xml", 
                "TestApplication.BindingInfo_parsed.xml", ".", "2010");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2010");

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2010");

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddresses.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }

        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.xml")]
        [DeploymentItem("Verify.MockAddresses.cs.txt")]
        public void TestInlineParsing_SimpleMock_MockedFileWriter_BTS2010()
        {            
            //Creating a mock for the file writer
            var fileWriterMock = new Mock<IFileWriter>();
            fileWriterMock.Setup(m => m.WriteTextFile(
                It.Is<string>(path => path.EndsWith("TestApplicationMockAddresses.cs")),
                It.Is<string>(contents => contents != null)));

            BizTalkBindingsParser parser = new BizTalkBindingsParser(fileWriterMock.Object);

            parser.ParseBindings("TestApplication.BindingInfo.xml", 
                "TestApplication.BindingInfo_parsed.xml", null, "2010");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2010");

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2010");
            }

            //Verifying the contents of the generated class
            using (var srVerify = System.IO.File.OpenText("Verify.MockAddresses.cs.txt"))
            {   
                string classVerify = srVerify.ReadToEnd();

                //Verifying the contents of the generated class
                fileWriterMock.Verify(m => m.WriteTextFile(
                    It.Is<string>(path => path.EndsWith("TestApplicationMockAddresses.cs")),
                    It.Is<string>(contents => contents == classVerify)), 
                    Times.Once());
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.BTDF.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddresses.cs.txt")]
        public void TestInlineParsing_SimpleMock_BTDFdirectives_BTS2010()
        {
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfo.BTDF.xml", "TestApplication.BindingInfo.BTDF_parsed.xml", null, "2010");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo.BTDF_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2010");

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2010");

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddresses.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }

        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfoWithProps.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddressesWithProps.cs.txt")]
        public void TestInlineParsing_WithPromotedProperties_BTS2010()
        {            
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfoWithProps.xml", 
                "TestApplication.BindingInfoWithProps_parsed.xml", null, "2010");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfoWithProps_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2010");

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            string expectedPromotedProperties = null;
            foreach (var receiveLocationElement in receiveLocationElements)
            {

                if (receiveLocationElement.Attribute("Name").Value == "OneWayReceive_FILE")
                {
                    expectedPromotedProperties = "FILE.ReceivedFileName=TestFileName.xml;";
                }
                else
                {
                    expectedPromotedProperties = "WCF.FakeAction=TestAction;WCF.FakeAddress=TestAddress;";
                }

                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2010",
                    expectedPromotedProperties);
                
            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddressesWithProps.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfoWithProps.BTDF.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddressesWithProps.cs.txt")]
        public void TestInlineParsing_WithPromotedProperties_BTDFdirectives_BTS2010()
        {
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfoWithProps.BTDF.xml",
                "TestApplication.BindingInfoWithProps.BTDF_parsed.xml", null, "2010");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfoWithProps.BTDF_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2010");

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            string expectedPromotedProperties = null;
            foreach (var receiveLocationElement in receiveLocationElements)
            {

                if (receiveLocationElement.Attribute("Name").Value == "OneWayReceive_FILE")
                {
                    expectedPromotedProperties = "FILE.ReceivedFileName=TestFileName.xml;";
                }
                else
                {
                    expectedPromotedProperties = "WCF.FakeAction=TestAction;WCF.FakeAddress=TestAddress;";
                }

                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2010",
                    expectedPromotedProperties);

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddressesWithProps.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.CustomURL.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddresses.CustomURL.cs.txt")]
        public void TestInlineParsing_SimpleMock_CustomURL_BTS2010()
        {
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfo.CustomURL.xml", "TestApplication.BindingInfo.CustomURL_parsed.xml", null, "2010");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo.CustomURL_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2010", 
                    hasCustomUrl: true);

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2010", 
                    hasCustomUrl: true);

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddresses.CustomURL.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }

        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddresses.cs.txt")]
        public void TestInlineParsing_SimpleMock_BTS2013()
        {
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfo.xml", 
                "TestApplication.BindingInfo2013_parsed.xml", "2013", false);

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo2013_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2013");

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2013");

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddresses.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }

        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddresses.cs.txt")]
        public void TestInlineParsing_SimpleMock_Unescape_BTS2013()
        {
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfo.xml", 
                "TestApplication.BindingInfo2013_parsed.xml", "2013", true);

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo2013_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2013", true);

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2013", null, true);

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddresses.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }

        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddresses.cs.txt")]
        public void TestInlineParsing_SimpleMock_ClassOutputSpecified_BTS2013()
        {
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfo.xml",
                "TestApplication.BindingInfo2013_parsed.xml", ".", "2013");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo2013_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2013");

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2013");

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddresses.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }

        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.BTDF.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddresses.cs.txt")]
        public void TestInlineParsing_SimpleMock_BTDFdirectives_BTS2013()
        {
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfo.BTDF.xml",
                "TestApplication.BindingInfo2013.BTDF_parsed.xml", "2013", false);

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo2013.BTDF_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2013");

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2013");

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddresses.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }

        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.CustomURL.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddresses.CustomURL.cs.txt")]
        public void TestInlineParsing_SimpleMock_CustomURL_BTS2013()
        {
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfo.CustomURL.xml", 
                "TestApplication.BindingInfo.CustomURL_parsed.xml", 
                null, "2013");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo.CustomURL_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2013",
                    hasCustomUrl: true);

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2013",
                    hasCustomUrl: true);

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddresses.CustomURL.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }

        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfo.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddresses.cs.txt")]
        public void TestInlineParsing_SimpleMock_MockedFileWriter_BTS2013()
        {
            //Creating a mock for the file writer
            var fileWriterMock = new Mock<IFileWriter>();
            fileWriterMock.Setup(m => m.WriteTextFile(
                It.Is<string>(path => path.EndsWith("TestApplicationMockAddresses.cs")),
                It.Is<string>(contents => contents != null)));

            BizTalkBindingsParser parser = new BizTalkBindingsParser(fileWriterMock.Object);

            parser.ParseBindings("TestApplication.BindingInfo.xml",
                "TestApplication.BindingInfo2013_parsed.xml", "2013");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfo2013_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2013");

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            foreach (var receiveLocationElement in receiveLocationElements)
            {
                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2013");
            }

            //Verifying the contents of the generated class
            using (var srVerify = System.IO.File.OpenText("Verify.MockAddresses.cs.txt"))
            {
                string classVerify = srVerify.ReadToEnd();

                fileWriterMock.Verify(m => m.WriteTextFile(
                    It.Is<string>(path => path.EndsWith("TestApplicationMockAddresses.cs")),
                    It.Is<string>(contents => contents == classVerify)), Times.Once());
            }
            //Verifying the contents of the generated class
            
        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfoWithProps.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddressesWithProps.cs.txt")]
        public void TestInlineParsing_WithPromotedProperties_BTS2013()
        {
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfoWithProps.xml",
                "TestApplication.BindingInfoWithProps2013_parsed.xml", "2013");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfoWithProps2013_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2013");

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            string expectedPromotedProperties = null;
            foreach (var receiveLocationElement in receiveLocationElements)
            {

                if (receiveLocationElement.Attribute("Name").Value == "OneWayReceive_FILE")
                {
                    expectedPromotedProperties = "FILE.ReceivedFileName=TestFileName.xml;";
                }
                else
                {
                    expectedPromotedProperties = "WCF.FakeAction=TestAction;WCF.FakeAddress=TestAddress;";
                }

                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2013",
                    expectedPromotedProperties);

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddressesWithProps.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }
        }

        [TestMethod]
        [DeploymentItem(@"TestData\TestApplication.BindingInfoWithProps.BTDF.xml")]
        [DeploymentItem(@"TestData\Verify.MockAddressesWithProps.cs.txt")]
        public void TestInlineParsing_WithPromotedProperties_BTDFdirectives_BTS2013()
        {
            BizTalkBindingsParser parser = new BizTalkBindingsParser();

            parser.ParseBindings("TestApplication.BindingInfoWithProps.BTDF.xml",
                "TestApplication.BindingInfoWithProps2013.BTDF_parsed.xml", "2013");

            XDocument parsedBindingsDoc = XDocument.Load("./TestApplication.BindingInfoWithProps2013.BTDF_parsed.xml");

            //asserting the send ports
            var sendPortElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "SendPort");

            foreach (var sendPortElement in sendPortElements)
            {
                VerifySendPortConfig(sendPortElement, "BTS2013");

            }
            //asserting the receive locations
            var receiveLocationElements = parsedBindingsDoc.Root.Descendants()
                .Where(e => e.Name == "ReceiveLocation");

            string expectedPromotedProperties = null;
            foreach (var receiveLocationElement in receiveLocationElements)
            {

                if (receiveLocationElement.Attribute("Name").Value == "OneWayReceive_FILE")
                {
                    expectedPromotedProperties = "FILE.ReceivedFileName=TestFileName.xml;";
                }
                else
                {
                    expectedPromotedProperties = "WCF.FakeAction=TestAction;WCF.FakeAddress=TestAddress;";
                }

                VerifyReceiceLocationConfig(receiveLocationElement, "BTS2013",
                    expectedPromotedProperties);

            }

            //Verifying the contents of the generated class
            using (System.IO.StreamReader sr = System.IO.File.OpenText("TestApplicationMockAddresses.cs"),
                srVerify = System.IO.File.OpenText("Verify.MockAddressesWithProps.cs.txt"))
            {
                string classContents = sr.ReadToEnd();
                string classVerify = srVerify.ReadToEnd();

                Assert.AreEqual(classVerify, classContents,
                    "The generated MockAddresses class has wrong contents");
            }
        }


        private void VerifyReceiceLocationConfig(XElement receiveLocationElement, 
            string btsVersion, 
            string promotedProperties=null,
            bool unescape = false,
            bool hasCustomUrl = false)
        {
            string receiveLocationName = receiveLocationElement.Attribute("Name").Value;
            bool isTwoWay = bool.Parse(receiveLocationElement.Parent.Parent.Attribute("IsTwoWay").Value);
            //fetch the primary transport element
            // var receiveLocationTransportElement = receiveLocationElement.Element("ReceiveLocationTransportType");
            //assert the address
            string address = receiveLocationElement.Element("Address").Value;

            if (hasCustomUrl)
            {
                string customUrl = expectedCustomURLs[receiveLocationName];

                Assert.IsNotNull(customUrl, "Custom URL noe specified for receive location " + receiveLocationName);

                Assert.AreEqual(string.Format("mock://localhost/{0}", customUrl),
                   address, "The custom address is not correct");
            }
            else
            {
                Assert.AreEqual(string.Format("mock://localhost/{0}", receiveLocationName), 
                    address, "The address is not correct");
            }

            var receiveHandler = receiveLocationElement.Element("ReceiveHandler");
             
            //Assert the transport type settings
            var transportTypeElement = receiveLocationElement.Element("ReceiveLocationTransportType");

            VerifyReceiveLocationHandler(transportTypeElement);

            //Assert the receive handler transport type settings
            var handlerTransportTypeElement = receiveHandler.Element("TransportType");

            VerifyReceiveLocationHandler(handlerTransportTypeElement);
            //Assert the transport type data
            var transportData = receiveLocationElement.Element("ReceiveLocationTransportTypeData");
            string expectedTransportData = null;

            string transportDataKey = null;
            if (isTwoWay)
            {
                transportDataKey = string.Format("{0}_ReceiveLocationTwoWays", btsVersion);
            }
            else
            {
                transportDataKey = string.Format("{0}_ReceiveLocationOneWay", btsVersion);
            }

            expectedTransportData = expectedTransportConfig[transportDataKey];
            expectedTransportData = expectedTransportData
                   .Replace("{Encoding}", "UTF-8")
                   .Replace("{PromotedProperties}", promotedProperties ?? string.Empty);

            if (unescape)
            {
                expectedTransportData = expectedTransportData
                    .Replace("&amp;", "&")
                    .Replace("&lt;", "<")
                    .Replace("&gt;", ">");
            }

            Assert.AreEqual(expectedTransportData,
                transportData.Value, 
                "Transport type data is not correct for receive location:" + receiveLocationName);
        }

        private void VerifyReceiveLocationHandler(XElement configElement)
        {            
            {
                Assert.AreEqual("WCF-Custom",
                configElement.Attribute("Name").Value,
                "Transport type name is not correct");

                Assert.AreEqual("907",
                    configElement.Attribute("Capabilities").Value,
                    "Transport type capabilities is not correct");

                Assert.AreEqual("af081f69-38ca-4d5b-87df-f0344b12557a",
                    configElement.Attribute("ConfigurationClsid").Value,
                    "Transport type configuraiton is not correct");
            }            
        }

        private void VerifySendPortConfig(XElement sendPortElement, 
            string btsVersion,
            bool unescape=false,
            bool hasCustomUrl=false)
        {
            string sendPortName = sendPortElement.Attribute("Name").Value;
            bool isTwoWay = bool.Parse(sendPortElement.Attribute("IsTwoWay").Value);
            //fetch the primary transport element
            var primaryTransportElement = sendPortElement.Element("PrimaryTransport");

            if (primaryTransportElement == null)
            {
                //no further verification since it is a case of dynamic send port
                return;
            }

            //assert the address
            string address = primaryTransportElement.Element("Address").Value;

            if (hasCustomUrl)
            {
                string customUrl = expectedCustomURLs[sendPortName];

                Assert.IsNotNull(customUrl, "Custom URL noe specified for send port " + sendPortName);

                Assert.AreEqual(string.Format("mock://localhost/{0}", customUrl),
                   address, "The custom address is not correct");
            }
            else
            {
                Assert.AreEqual(string.Format("mock://localhost/{0}", sendPortName),
                   address, "The address is not correct");
            }
            
            //Assert the transport type settings
            var transportTypeElement = primaryTransportElement.Element("TransportType");

            VerifySendPortHandlerConfig(transportTypeElement);

            //Assert the send handler transport type settings
            var handlerTransportTypeElement = primaryTransportElement.Descendants()
                .Where(e => e.Name == "TransportType" && e.Parent.Name == "SendHandler").First();

            VerifySendPortHandlerConfig(handlerTransportTypeElement);

            //Assert the transport type data
            string transportDataKey = null;
            var transportData = primaryTransportElement.Element("TransportTypeData");
            if (isTwoWay)
            {
                transportDataKey = string.Format("{0}_SendPortTwoWays", btsVersion);
            }
            else
            {
                transportDataKey = string.Format("{0}_SendPortOneWay", btsVersion);
            }

            string expectedTransportData = expectedTransportConfig[transportDataKey];

            if (unescape)
            {
                expectedTransportData = expectedTransportData
                    .Replace("&amp;", "&")
                    .Replace("&lt;", "<")
                    .Replace("&gt;", ">");
            }

            Assert.AreEqual(expectedTransportData.Replace("{Encoding}", "UTF-8"), 
                transportData.Value,                  
                "Transport type data is not correct for send port:" + sendPortName);
        }

        private void VerifySendPortHandlerConfig(XElement configElement)
        {
            Assert.AreEqual("WCF-Custom",
                configElement.Attribute("Name").Value,
                "Transport type name is not correct");

            Assert.AreEqual("907",
                configElement.Attribute("Capabilities").Value,
                "Transport type capabilities is not correct");

            Assert.AreEqual("af081f69-38ca-4d5b-87df-f0344b12557a",
                configElement.Attribute("ConfigurationClsid").Value,
                "Transport type configuraiton is not correct");
        }

        private static string CreateBTS2013TwoWaySendPortConfig()
        {
            StringBuilder sb = new StringBuilder(256);

            sb.Append("<CustomProps>")
                .Append("<BindingType vt=\"8\">mockBinding</BindingType>")
                .Append("<BindingConfiguration vt=\"8\">&lt;binding name=\"mockBinding\" Encoding=\"{Encoding}\" /&gt;</BindingConfiguration>")
                .Append("<InboundBodyPathExpression vt=\"8\">/MessageContent</InboundBodyPathExpression>")
                .Append("<OutboundBodyLocation vt=\"8\">UseTemplate</OutboundBodyLocation>")
                .Append("<AffiliateApplicationName vt=\"8\" />")
                .Append("<StaticAction vt=\"8\" />")
                .Append("<ProxyUserName vt=\"8\" />")
                .Append("<ProxyAddress vt=\"8\" />")
                .Append("<UserName vt=\"8\" />")
                .Append("<InboundBodyLocation vt=\"8\">UseBodyPath</InboundBodyLocation>")
                .Append("<EndpointBehaviorConfiguration vt=\"8\">&lt;behavior name=\"EndpointBehavior\" /&gt;</EndpointBehaviorConfiguration>")
                .Append("<OutboundXmlTemplate vt=\"8\">&lt;bts-msg-body xmlns=\"http://www.microsoft.com/schemas/bts2007\" encoding=\"base64\"/&gt;</OutboundXmlTemplate>")
                .Append("<PropagateFaultMessage vt=\"11\">-1</PropagateFaultMessage>")
                .Append("<InboundNodeEncoding vt=\"8\">Base64</InboundNodeEncoding>")
                .Append("<IsolationLevel vt=\"8\">Serializable</IsolationLevel>")
                .Append("<UseSSO vt=\"11\">0</UseSSO>")
                .Append("<EnableTransaction vt=\"11\">0</EnableTransaction>")
                .Append("</CustomProps>");

            return sb.ToString();
        }

        private static string CreateBTS2013OneWaySendPortConfig()
        {
            StringBuilder sb = new StringBuilder(256);

            sb.Append("<CustomProps>")
                .Append("<BindingType vt=\"8\">mockBinding</BindingType>")
                .Append("<BindingConfiguration vt=\"8\">&lt;binding name=\"mockBinding\" Encoding=\"{Encoding}\" /&gt;</BindingConfiguration>")
                .Append("<InboundBodyPathExpression vt=\"8\" />")
                .Append("<OutboundBodyLocation vt=\"8\">UseTemplate</OutboundBodyLocation>")
                .Append("<AffiliateApplicationName vt=\"8\" />")
                .Append("<StaticAction vt=\"8\" />")
                .Append("<ProxyUserName vt=\"8\" />")
                .Append("<ProxyAddress vt=\"8\" />")
                .Append("<UserName vt=\"8\" />")
                .Append("<InboundBodyLocation vt=\"8\">UseBodyElement</InboundBodyLocation>")
                .Append("<EndpointBehaviorConfiguration vt=\"8\">&lt;behavior name=\"EndpointBehavior\" /&gt;</EndpointBehaviorConfiguration>")
                .Append("<OutboundXmlTemplate vt=\"8\">&lt;bts-msg-body xmlns=\"http://www.microsoft.com/schemas/bts2007\" encoding=\"base64\"/&gt;</OutboundXmlTemplate>")
                .Append("<PropagateFaultMessage vt=\"11\">-1</PropagateFaultMessage>")
                .Append("<InboundNodeEncoding vt=\"8\">Xml</InboundNodeEncoding>")
                .Append("<IsolationLevel vt=\"8\">Serializable</IsolationLevel>")
                .Append("<UseSSO vt=\"11\">0</UseSSO>")
                .Append("<EnableTransaction vt=\"11\">0</EnableTransaction>")
                .Append("</CustomProps>");

            return sb.ToString();
        }

        private static string CreateBTS2013TwoWayReceiveLocationConfig()
        {
            StringBuilder sb = new StringBuilder(256);

            sb.Append("<CustomProps>")
                .Append("<BindingConfiguration vt=\"8\">&lt;binding name=\"mockBinding\" Encoding=\"{Encoding}\" PromotedProperties=\"{PromotedProperties}\" /&gt;</BindingConfiguration>")
                .Append("<InboundBodyPathExpression vt=\"8\">/MessageContent</InboundBodyPathExpression>")
                .Append("<InboundBodyLocation vt=\"8\">UseBodyPath</InboundBodyLocation>")
                .Append("<AffiliateApplicationName vt=\"8\" />")
                .Append("<BindingType vt=\"8\">mockBinding</BindingType>")
                .Append("<DisableLocationOnFailure vt=\"11\">0</DisableLocationOnFailure>")
                .Append("<UserName vt=\"8\" />")
                .Append("<ServiceBehaviorConfiguration vt=\"8\">&lt;behavior name=\"ServiceBehavior\" /&gt;</ServiceBehaviorConfiguration>")
                .Append("<EndpointBehaviorConfiguration vt=\"8\">&lt;behavior name=\"EndpointBehavior\" /&gt;</EndpointBehaviorConfiguration>")
                .Append("<OutboundXmlTemplate vt=\"8\">&lt;bts-msg-body xmlns=\"http://www.microsoft.com/schemas/bts2007\" encoding=\"base64\"/&gt;</OutboundXmlTemplate>")
                .Append("<IncludeExceptionDetailInFaults vt=\"11\">-1</IncludeExceptionDetailInFaults>")
                .Append("<InboundNodeEncoding vt=\"8\">Base64</InboundNodeEncoding>")
                .Append("<CredentialType vt=\"8\">None</CredentialType>")
                .Append("<OutboundBodyLocation vt=\"8\">UseTemplate</OutboundBodyLocation>")
                .Append("<SuspendMessageOnFailure vt=\"11\">-1</SuspendMessageOnFailure>")
                .Append("<OrderedProcessing vt=\"11\">0</OrderedProcessing>")
                .Append("</CustomProps>");

            return sb.ToString();
        }

        private static string CreateBTS2013OneWayReceiveLocation()
        {
            StringBuilder sb = new StringBuilder(256);

            sb.Append("<CustomProps>")
                .Append("<BindingConfiguration vt=\"8\">&lt;binding name=\"mockBinding\" Encoding=\"{Encoding}\" PromotedProperties=\"{PromotedProperties}\" /&gt;</BindingConfiguration>")
                .Append("<InboundBodyPathExpression vt=\"8\">/MessageContent</InboundBodyPathExpression>")
                .Append("<InboundBodyLocation vt=\"8\">UseBodyPath</InboundBodyLocation>")
                .Append("<AffiliateApplicationName vt=\"8\" />")
                .Append("<BindingType vt=\"8\">mockBinding</BindingType>")
                .Append("<DisableLocationOnFailure vt=\"11\">0</DisableLocationOnFailure>")
                .Append("<UserName vt=\"8\" />")
                .Append("<ServiceBehaviorConfiguration vt=\"8\">&lt;behavior name=\"ServiceBehavior\" /&gt;</ServiceBehaviorConfiguration>")
                .Append("<EndpointBehaviorConfiguration vt=\"8\">&lt;behavior name=\"EndpointBehavior\" /&gt;</EndpointBehaviorConfiguration>")
                .Append("<OutboundXmlTemplate vt=\"8\">&lt;bts-msg-body xmlns=\"http://www.microsoft.com/schemas/bts2007\" encoding=\"xml\"/&gt;</OutboundXmlTemplate>")
                .Append("<IncludeExceptionDetailInFaults vt=\"11\">-1</IncludeExceptionDetailInFaults>")
                .Append("<InboundNodeEncoding vt=\"8\">Base64</InboundNodeEncoding>")
                .Append("<CredentialType vt=\"8\">None</CredentialType>")
                .Append("<OutboundBodyLocation vt=\"8\">UseBodyElement</OutboundBodyLocation>")
                .Append("<SuspendMessageOnFailure vt=\"11\">-1</SuspendMessageOnFailure>")
                .Append("<OrderedProcessing vt=\"11\">0</OrderedProcessing>")
                .Append("</CustomProps>");

            return sb.ToString();
        }

        private static string CreateBTS2010TwoWaySendPortConfig()
        {
            StringBuilder sb = new StringBuilder(256);

            sb.Append("<CustomProps>")
                .Append("<BindingConfiguration vt=\"8\">&lt;binding name=\"mockBinding\" Encoding=\"{Encoding}\" /&gt;</BindingConfiguration>")
                .Append("<InboundBodyPathExpression vt=\"8\">/MessageContent</InboundBodyPathExpression>")
                .Append("<OutboundBodyLocation vt=\"8\">UseTemplate</OutboundBodyLocation>")
                .Append("<AffiliateApplicationName vt=\"8\" />")
                .Append("<EnableTransaction vt=\"11\">0</EnableTransaction>")
                .Append("<StaticAction vt=\"8\" />")
                .Append("<BindingType vt=\"8\">mockBinding</BindingType>")
                .Append("<ProxyAddress vt=\"8\" />")
                .Append("<UserName vt=\"8\" />")
                .Append("<InboundBodyLocation vt=\"8\">UseBodyPath</InboundBodyLocation>")
                .Append("<EndpointBehaviorConfiguration vt=\"8\">&lt;behavior name=\"EndpointBehavior\" /&gt;</EndpointBehaviorConfiguration>")
                .Append("<OutboundXmlTemplate vt=\"8\">&lt;bts-msg-body xmlns=\"http://www.microsoft.com/schemas/bts2007\" encoding=\"base64\"/&gt;</OutboundXmlTemplate>")
                .Append("<PropagateFaultMessage vt=\"11\">-1</PropagateFaultMessage>")
                .Append("<InboundNodeEncoding vt=\"8\">Base64</InboundNodeEncoding>")
                .Append("<ProxyUserName vt=\"8\" />")
                .Append("<IsolationLevel vt=\"8\">Serializable</IsolationLevel>")
                .Append("<UseSSO vt=\"11\">0</UseSSO>")
                .Append("</CustomProps>");

            return sb.ToString();
        }

        private static string CreateBTS2010OneWaySendPortConfig()
        {
            StringBuilder sb = new StringBuilder(256);

            sb.Append("<CustomProps>")
                .Append("<BindingConfiguration vt=\"8\">&lt;binding name=\"mockBinding\" Encoding=\"{Encoding}\" /&gt;</BindingConfiguration>")
                .Append("<InboundBodyPathExpression vt=\"8\" />")
                .Append("<OutboundBodyLocation vt=\"8\">UseTemplate</OutboundBodyLocation>")
                .Append("<AffiliateApplicationName vt=\"8\" />")
                .Append("<EnableTransaction vt=\"11\">0</EnableTransaction>")
                .Append("<StaticAction vt=\"8\" />")
                .Append("<BindingType vt=\"8\">mockBinding</BindingType>")
                .Append("<ProxyAddress vt=\"8\" />")
                .Append("<UserName vt=\"8\" />")
                .Append("<InboundBodyLocation vt=\"8\">UseBodyElement</InboundBodyLocation>")
                .Append("<EndpointBehaviorConfiguration vt=\"8\">&lt;behavior name=\"EndpointBehavior\" /&gt;</EndpointBehaviorConfiguration>")
                .Append("<OutboundXmlTemplate vt=\"8\">&lt;bts-msg-body xmlns=\"http://www.microsoft.com/schemas/bts2007\" encoding=\"base64\"/&gt;</OutboundXmlTemplate>")
                .Append("<PropagateFaultMessage vt=\"11\">-1</PropagateFaultMessage>")
                .Append("<InboundNodeEncoding vt=\"8\">Xml</InboundNodeEncoding>")
                .Append("<ProxyUserName vt=\"8\" />")
                .Append("<IsolationLevel vt=\"8\">Serializable</IsolationLevel>")
                .Append("<UseSSO vt=\"11\">0</UseSSO>")
                .Append("</CustomProps>");

            return sb.ToString();
        }

        private static string CreateBTS2010TwoWayReceiveLocationConfig()
        {
            StringBuilder sb = new StringBuilder(256);

            sb.Append("<CustomProps>")
                .Append("<OrderedProcessing vt=\"11\">0</OrderedProcessing>")
                .Append("<InboundBodyLocation vt=\"8\">UseBodyPath</InboundBodyLocation>")
                .Append("<InboundBodyPathExpression vt=\"8\">/MessageContent</InboundBodyPathExpression>")
                .Append("<OutboundBodyLocation vt=\"8\">UseTemplate</OutboundBodyLocation>")
                .Append("<AffiliateApplicationName vt=\"8\" />")
                .Append("<BindingType vt=\"8\">mockBinding</BindingType>")
                .Append("<DisableLocationOnFailure vt=\"11\">0</DisableLocationOnFailure>")
                .Append("<UserName vt=\"8\" />")
                .Append("<ServiceBehaviorConfiguration vt=\"8\">&lt;behavior name=\"ServiceBehavior\" /&gt;</ServiceBehaviorConfiguration>")
                .Append("<EndpointBehaviorConfiguration vt=\"8\">&lt;behavior name=\"EndpointBehavior\" /&gt;</EndpointBehaviorConfiguration>")
                .Append("<OutboundXmlTemplate vt=\"8\">&lt;bts-msg-body xmlns=\"http://www.microsoft.com/schemas/bts2007\" encoding=\"base64\"/&gt;</OutboundXmlTemplate>")
                .Append("<IncludeExceptionDetailInFaults vt=\"11\">-1</IncludeExceptionDetailInFaults>")
                .Append("<InboundNodeEncoding vt=\"8\">Base64</InboundNodeEncoding>")
                .Append("<CredentialType vt=\"8\">None</CredentialType>")
                .Append("<BindingConfiguration vt=\"8\">&lt;binding name=\"mockBinding\" Encoding=\"{Encoding}\" PromotedProperties=\"{PromotedProperties}\" /&gt;</BindingConfiguration>")
                .Append("<SuspendMessageOnFailure vt=\"11\">-1</SuspendMessageOnFailure>")
                .Append("</CustomProps>");

            return sb.ToString();
        }

        private static string CreateBTS2010OneWayReceiveLocationConfig()
        {
            StringBuilder sb = new StringBuilder(256);

            sb.Append("<CustomProps>")
                .Append("<OrderedProcessing vt=\"11\">0</OrderedProcessing>")
                .Append("<InboundBodyLocation vt=\"8\">UseBodyPath</InboundBodyLocation>")
                .Append("<InboundBodyPathExpression vt=\"8\">/MessageContent</InboundBodyPathExpression>")
                .Append("<OutboundBodyLocation vt=\"8\">UseBodyElement</OutboundBodyLocation>")
                .Append("<AffiliateApplicationName vt=\"8\" />")
                .Append("<BindingType vt=\"8\">mockBinding</BindingType>")
                .Append("<DisableLocationOnFailure vt=\"11\">0</DisableLocationOnFailure>")
                .Append("<UserName vt=\"8\" />")
                .Append("<ServiceBehaviorConfiguration vt=\"8\">&lt;behavior name=\"ServiceBehavior\" /&gt;</ServiceBehaviorConfiguration>")
                .Append("<EndpointBehaviorConfiguration vt=\"8\">&lt;behavior name=\"EndpointBehavior\" /&gt;</EndpointBehaviorConfiguration>")
                .Append("<OutboundXmlTemplate vt=\"8\">&lt;bts-msg-body xmlns=\"http://www.microsoft.com/schemas/bts2007\" encoding=\"xml\"/&gt;</OutboundXmlTemplate>")
                .Append("<IncludeExceptionDetailInFaults vt=\"11\">-1</IncludeExceptionDetailInFaults>")
                .Append("<InboundNodeEncoding vt=\"8\">Base64</InboundNodeEncoding>")
                .Append("<CredentialType vt=\"8\">None</CredentialType>")
                .Append("<BindingConfiguration vt=\"8\">&lt;binding name=\"mockBinding\" Encoding=\"{Encoding}\" PromotedProperties=\"{PromotedProperties}\" /&gt;</BindingConfiguration>")
                .Append("<SuspendMessageOnFailure vt=\"11\">-1</SuspendMessageOnFailure>")
                .Append("</CustomProps>");

            return sb.ToString();
        }
    }

    internal class MockTag
    {
        public string Encoding { get; set; }

        public string Operation { get; set; }
    }
}
