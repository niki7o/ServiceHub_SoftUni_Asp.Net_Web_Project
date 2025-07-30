using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceHub.Common
{
    public class ServiceConstants
    {
        public static readonly Guid FileConverterServiceId = Guid.Parse("1D4AE40B-C305-47B7-BEED-163C4A0AEB40"); 

        public static readonly Guid CodeSnippetConverterServiceId = Guid.Parse("E11E539C-0290-4171-B606-16628D1790B0");

        public static readonly Guid TextCaseConverterServiceId = Guid.Parse("C10DE2FA-B49B-4C0D-9E8F-142B3CD40E6F");

        public static readonly Guid AutoCvResumeServiceId = Guid.Parse("F0C72C7B-709D-44B7-81C1-1E5AB73305EC");

        public static readonly Guid RandomPasswordGeneratorServiceId = Guid.Parse("F5E402C0-91BA-4F8E-97D0-3B443FE10D3C");


        public static readonly Guid InvoiceReceiptGeneratorId = Guid.Parse("B422F89B-E7A3-4130-B899-7B56010007E0"); 

        public static readonly Guid FinancialCalculatorAnalyzerId = Guid.Parse("2EF43D87-D749-4D7D-9B7D-F7C4F527BEA7"); 

        public static readonly Guid WordCharacterCounterServiceId = Guid.Parse("3A7B8B0C-1D2E-4F5A-A837-3D5E9F1A2B0C");

        public static readonly Guid ContractGeneratorServiceId = Guid.Parse("8EDC2D04-00F5-4630-B5A9-4FA499FC7210");

    }
}
