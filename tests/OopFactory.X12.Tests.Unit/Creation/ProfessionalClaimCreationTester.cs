﻿using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OopFactory.X12.Parsing.Model;
using OopFactory.X12.Parsing.Model.Typed;

namespace OopFactory.X12.Tests.Unit.Creation
{
    [TestClass]
    public class ProfessionalClaimCreationTester
    {
        [TestMethod]
        public void Create837_5010Version()
        {
            var message = new Interchange(Convert.ToDateTime("01/01/03"), 000905, false)
                              {
                                  InterchangeSenderIdQualifier = "ZZ",
                                  InterchangeSenderId = "SUBMITTERS.ID",
                                  InterchangeReceiverIdQualifier = "ZZ",
                                  InterchangeReceiverId = "RECEIVERS.ID"
                              };
            message.SetElement(12, "00501");
            var group = message.AddFunctionGroup("HC", DateTime.Now, 000905, "005010X222");
            group.ApplicationSendersCode = "901234572000";
            group.ApplicationReceiversCode = "908887732000";

            var transaction = group.AddTransaction("837", "0034");
            transaction.SetElement(2, "5010X837");
            var bhtSegment = transaction.AddSegment("BHT");

            var submitterLoop = transaction.AddLoop(new TypedLoopNM1("41")); //submitter identifier code
            submitterLoop.NM102_EntityTypeQualifier = EntityTypeQualifier.NonPersonEntity;
            submitterLoop.NM103_NameLastOrOrganizationName = "My Submitter";
            submitterLoop.NM104_NameFirst = "First Name < 25 Chars";

            var perSegment = submitterLoop.AddSegment(new TypedSegmentPER());
            perSegment.PER01_ContactFunctionCode = "IC"; //information contact function code
            perSegment.PER02_Name = "My Contact";
            perSegment.PER03_CommunicationNumberQualifier = CommunicationNumberQualifer.Telephone;
            perSegment.PER04_CommunicationNumber = "18885551212";

            var provider2000AHLoop = transaction.AddHLoop("1", "20", true); //Information Source
            provider2000AHLoop.AddSegment("PRV"); //Specialty Segment
            var provider2010AALoop = provider2000AHLoop.AddLoop(new TypedLoopNM1("85"));  // changed this from PE to 85 because in the spec xml for the LoopId 2000A Starting Segment NM1 the EntityIdentifier value is 85
            provider2010AALoop.NM102_EntityTypeQualifier = EntityTypeQualifier.Person;
            provider2010AALoop.NM103_NameLastOrOrganizationName = "DOE";
            provider2010AALoop.NM104_NameFirst = "JOHN";

            var provider2010ACLoop = provider2000AHLoop.AddLoop(new TypedLoopNM1("85"));  // I think this is looking for the same element 85 as above
            provider2010ACLoop.NM102_EntityTypeQualifier = EntityTypeQualifier.NonPersonEntity;
            provider2010ACLoop.NM103_NameLastOrOrganizationName = "Pay-To Plan Name";
            var provider2010AC_N3Segment = provider2010ACLoop.AddSegment(new TypedSegmentN3());
            provider2010AC_N3Segment.N301_AddressInformation = "1234 Main St";

            var provider2010AC_N4Segment = provider2010ACLoop.AddSegment(new TypedSegmentN4());
            provider2010AC_N4Segment.N401_CityName = "Beverley Hills";
            provider2010AC_N4Segment.N402_StateOrProvinceCode = "CA";
            provider2010AC_N4Segment.N403_PostalCode = "90210";

            var subscriber2000BHLoop = provider2000AHLoop.AddHLoop("2", "22", false);
            var subscriberName2010BALoop = subscriber2000BHLoop.AddLoop(new TypedLoopNM1("IL"));

            //******************************* add N3 and N4 here

            var segmentN3 = subscriberName2010BALoop.AddSegment(new TypedSegmentN3());

            var segmentN4 = subscriberName2010BALoop.AddSegment(new TypedSegmentN4());

            var subscriber_DMGSegment = subscriberName2010BALoop.AddSegment(new TypedSegmentDMG());
            subscriber_DMGSegment.DMG01_DateTimePeriodFormatQualifier = "D8";
            subscriber_DMGSegment.DMG02_DateOfBirth = DateTime.Parse("5/1/1973");
            subscriber_DMGSegment.DMG03_Gender = Gender.Male;

            var claim2300Loop = subscriber2000BHLoop.AddLoop(new TypedLoopCLM());
            claim2300Loop.CLM01_PatientControlNumber = "26463774";
            claim2300Loop.CLM02_TotalClaimChargeAmount = Convert.ToDecimal(100);
            claim2300Loop.CLM05._1_FacilityCodeValue = "11";
            claim2300Loop.CLM05._2_FacilityCodeQualifier = "B";
            claim2300Loop.CLM05._3_ClaimFrequencyTypeCode = "1";
            claim2300Loop.CLM06_ProviderOrSupplierSignatureIndicator = true;
            claim2300Loop.CLM07_ProviderAcceptAssignmentCode = "A";
            claim2300Loop.CLM08_BenefitsAssignmentCerficationIndicator = "Y";
            claim2300Loop.CLM09_ReleaseOfInformationCode = "I";

            var refSegment = claim2300Loop.AddSegment(new TypedSegmentREF());
            refSegment.REF01_ReferenceIdQualifier = "D9";
            refSegment.REF02_ReferenceId = "17312345600006351";

            var hiSegment = claim2300Loop.AddSegment(new TypedSegmentHI());
            hiSegment.HI01_HealthCareCodeInformation = "BK:0340";
            hiSegment.HI02_HealthCareCodeInformation = "BF:V7389";

            var lxLoop = claim2300Loop.AddLoop(new TypedLoopLX("LX"));
            lxLoop.LX01_AssignedNumber = "1";

            var sv1Segment = lxLoop.AddSegment(new TypedSegmentSV1());
            sv1Segment.SV101_CompositeMedicalProcedure = "HC:99213";
            sv1Segment.SV102_MonetaryAmount = "40";
            sv1Segment.SV103_UnitBasisMeasCode = "UN";
            sv1Segment.SV104_Quantity = "1";
            sv1Segment.SV107_CompDiagCodePoint = "1";

            var dtpSegment = lxLoop.AddSegment(new TypedSegmentDTP());
            dtpSegment.DTP01_DateTimeQualifier = "472";
            dtpSegment.DTP02_DateTimePeriodFormatQualifier = "D8";
            DateTime theDate = DateTime.ParseExact("20061003", "yyyyMMdd", null);
            dtpSegment.DTP03_Date = theDate;

            var lxLoop2 = claim2300Loop.AddLoop(new TypedLoopLX("LX"));
            lxLoop2.LX01_AssignedNumber = "2";

            var sv1Segment2 = lxLoop2.AddSegment(new TypedSegmentSV1());
            sv1Segment2.SV101_CompositeMedicalProcedure = "HC:87070";
            sv1Segment2.SV102_MonetaryAmount = "15";
            sv1Segment2.SV103_UnitBasisMeasCode = "UN";
            sv1Segment2.SV104_Quantity = "1";
            sv1Segment2.SV107_CompDiagCodePoint = "1";

            var dtpSegment2 = lxLoop2.AddSegment(new TypedSegmentDTP());
            dtpSegment2.DTP01_DateTimeQualifier = "472";
            dtpSegment2.DTP02_DateTimePeriodFormatQualifier = "D8";
            DateTime theDate2 = DateTime.ParseExact("20061003", "yyyyMMdd", null);
            dtpSegment2.DTP03_Date = theDate2;

            var lxLoop3 = claim2300Loop.AddLoop(new TypedLoopLX("LX"));
            lxLoop3.LX01_AssignedNumber = "3";

            var sv1Segment3 = lxLoop3.AddSegment(new TypedSegmentSV1());
            sv1Segment3.SV101_CompositeMedicalProcedure = "HC:99214";
            sv1Segment3.SV102_MonetaryAmount = "35";
            sv1Segment3.SV103_UnitBasisMeasCode = "UN";
            sv1Segment3.SV104_Quantity = "1";
            sv1Segment3.SV107_CompDiagCodePoint = "2";

            var dtpSegment3 = lxLoop3.AddSegment(new TypedSegmentDTP());
            dtpSegment3.DTP01_DateTimeQualifier = "472";
            dtpSegment3.DTP02_DateTimePeriodFormatQualifier = "D8";
            DateTime theDate3 = DateTime.ParseExact("20061010", "yyyyMMdd", null);
            dtpSegment3.DTP03_Date = theDate3;

            var lxLoop4 = claim2300Loop.AddLoop(new TypedLoopLX("LX"));
            lxLoop4.LX01_AssignedNumber = "4";

            var sv1Segment4 = lxLoop4.AddSegment(new TypedSegmentSV1());
            sv1Segment4.SV101_CompositeMedicalProcedure = "HC:86663";
            sv1Segment4.SV102_MonetaryAmount = "10";
            sv1Segment4.SV103_UnitBasisMeasCode = "UN";
            sv1Segment4.SV104_Quantity = "1";
            sv1Segment4.SV107_CompDiagCodePoint = "2";

            var dtpSegment4 = lxLoop4.AddSegment(new TypedSegmentDTP());
            dtpSegment4.DTP01_DateTimeQualifier = "472";
            dtpSegment4.DTP02_DateTimePeriodFormatQualifier = "D8";
            DateTime theDate4 = DateTime.ParseExact("20061010", "yyyyMMdd", null);
            dtpSegment4.DTP03_Date = theDate4;
            var x12 = message.SerializeToX12(true);
            //Assert.AreEqual(new StreamReader(Extensions.GetEdi("INS._837P._5010.Example1_HealthInsurance.txt")).ReadToEnd(), message.SerializeToX12(true));
            Trace.Write(x12);
        }
    }
}