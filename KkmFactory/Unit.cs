using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace KkmFactory;

public class Unit
{
	public class iUnitSettings
	{
		public class VauePar
		{
			public string Value = "";

			public string Caption = "";
		}

		public class Paramert
		{
			public string Page = "";

			public string Group = "";

			public string Name = "";

			public string Caption = "";

			public string Help = "";

			public string Description = "";

			public string TypeValue = "";

			public string FieldFormat = "";

			public bool ReadOnly;

			public string DefaultValue = "";

			public string MasterParameterName = "";

			public string MasterParameterOperation = "";

			public string MasterParameterValue = "";

			public List<VauePar> VauePars = new List<VauePar>();

			public bool SaveOnChange;
		}

		public List<Paramert> Paramerts = new List<Paramert>();
	}

	[DataContract]
	public class DataCommand
	{
		[DataContract]
		public class PrintImage
		{
			[DataMember(Name = "Image")]
			public string Image = "";
		}

		[DataContract]
		public class PrintBarcode
		{
			[DataMember(Name = "BarcodeType")]
			public string BarcodeType = "";

			[DataMember(Name = "Barcode")]
			public string Barcode = "";
		}

		[DataContract]
		public class PrintString
		{
			[DataMember(Name = "Text")]
			public string Text = "";

			[DataMember(Name = "Font")]
			public int Font;

			[DataMember(Name = "Intensity")]
			public int Intensity;
		}

		[DataContract]
		public class Register
		{
			[DataContract]
			public class tGoodCodeData
			{
				[DataMember(Name = "BarCode")]
				public string BarCode;

				[DataMember(Name = "AcceptOnBad")]
				public bool? AcceptOnBad;

				[DataMember(Name = "ContainsSerialNumber")]
				public bool ContainsSerialNumber = true;

				[DataMember(Name = "MarkingCodeBase64")]
				public string MarkingCodeBase64;

				public string TryBarCode;

				[DataMember(Name = "StampType")]
				public string StampType = "";

				[DataMember(Name = "GTIN")]
				public string GTIN = "";

				[DataMember(Name = "SerialNumber")]
				public string SerialNumber = "";

				[DataMember(Name = "IndustryProps")]
				[DefaultValue(null)]
				public string IndustryProps;

				[DataMember(Name = "AddIndustryProps")]
				[DefaultValue(null)]
				public string AddIndustryProps;

				public string Props1262 = "030";

				public string Props1263 = "21.11.2023";

				public string Props1264 = "1944";
			}

			[DataMember(Name = "Name")]
			public string Name = "";

			[DataMember(Name = "Quantity")]
			public decimal Quantity;

			[DataMember(Name = "Price")]
			public decimal Price;

			[DataMember(Name = "Amount")]
			public decimal Amount;

			[DataMember(Name = "Department")]
			public int Department;

			[DataMember(Name = "Tax")]
			public decimal Tax;

			[DataMember(Name = "EAN13")]
			public string EAN13 = "";

			[DataMember(Name = "SignMethodCalculation")]
			public int? SignMethodCalculation;

			[DataMember(Name = "SignCalculationObject")]
			public int? SignCalculationObject;

			[DataMember(Name = "MeasurementUnit")]
			public string MeasurementUnit = "";

			[DataMember(Name = "MeasureOfQuantity")]
			[DefaultValue(null)]
			public uint? MeasureOfQuantity;

			[DataMember(Name = "PackageQuantity")]
			[DefaultValue(null)]
			public uint? PackageQuantity;

			[DataMember(Name = "GoodCodeData")]
			public tGoodCodeData GoodCodeData;

			[DataMember(Name = "AgentSign")]
			public int? AgentSign;

			[DataMember(Name = "AgentData")]
			public TypeAgentData AgentData;

			[DataMember(Name = "PurveyorData")]
			public TypePurveyorData PurveyorData;

			[DataMember(Name = "AdditionalAttribute")]
			public string AdditionalAttribute = "";

			[DataMember(Name = "CountryOfOrigin")]
			public string CountryOfOrigin;

			[DataMember(Name = "CustomsDeclaration")]
			public string CustomsDeclaration;

			[DataMember(Name = "ExciseAmount")]
			public decimal? ExciseAmount;

			public string StSkidka = "";

			public decimal Skidka;
		}

		[DataContract]
		public class GoodCodeData
		{
			[DataMember(Name = "Name")]
			public string Name;

			[DataMember(Name = "BarCode")]
			public string BarCode;

			[DataMember(Name = "MarkingCodeBase64")]
			public string MarkingCodeBase64;

			public string TryBarCode;

			[DataMember(Name = "AcceptOnBad")]
			public bool AcceptOnBad = true;

			[DataMember(Name = "WaitForResult")]
			public bool WaitForResult = true;

			[DataMember(Name = "MeasureOfQuantity")]
			[DefaultValue(null)]
			public uint? MeasureOfQuantity;

			[DataMember(Name = "Quantity")]
			[DefaultValue(1)]
			public decimal Quantity = 1m;

			[DataMember(Name = "PackageQuantity")]
			[DefaultValue(null)]
			public uint? PackageQuantity;

			[DataMember(Name = "Price")]
			[DefaultValue(-1)]
			public decimal Price = -1m;

			[DataMember(Name = "IndustryProps")]
			[DefaultValue(null)]
			public string IndustryProps;
		}

		[DataContract]
		public class CheckString
		{
			[DataMember(Name = "PrintImage")]
			public PrintImage PrintImage;

			[DataMember(Name = "BarCode")]
			public PrintBarcode BarCode;

			[DataMember(Name = "PrintText")]
			public PrintString PrintText;

			[DataMember(Name = "Register")]
			public Register Register;

			[DataMember(Name = "EndPage")]
			public bool EndPage;
		}

		[DataContract]
		public class CheckProp
		{
			[DataMember(Name = "Print")]
			public bool Print;

			[DataMember(Name = "PrintInHeader")]
			public bool PrintInHeader = true;

			[DataMember(Name = "Teg")]
			public int Teg;

			[DataMember(Name = "Prop")]
			public object Prop;
		}

		[DataContract]
		public class AdditionalProp
		{
			[DataMember(Name = "Print")]
			public bool Print;

			[DataMember(Name = "PrintInHeader")]
			public bool PrintInHeader = true;

			[DataMember(Name = "NameProp")]
			public string NameProp = "";

			[DataMember(Name = "Prop")]
			public string Prop = "";
		}

		[DataContract]
		public class TypeRegKkmOfd
		{
			[DataMember(Name = "Command", EmitDefaultValue = false)]
			[DefaultValue("")]
			public string Command = "";

			[DataMember(Name = "InnOrganization")]
			public string InnOrganization = "";

			[DataMember(Name = "NameOrganization")]
			public string NameOrganization = "";

			[DataMember(Name = "TaxVariant")]
			public string TaxVariant = "";

			[DataMember(Name = "AddressSettle")]
			public string AddressSettle = "";

			[DataMember(Name = "PlaceSettle")]
			public string PlaceSettle = "";

			[DataMember(Name = "SenderEmail")]
			public string SenderEmail = "";

			[DataMember(Name = "EncryptionMode")]
			public bool EncryptionMode;

			[DataMember(Name = "OfflineMode")]
			public bool OfflineMode;

			[DataMember(Name = "AutomaticMode")]
			public bool AutomaticMode;

			[DataMember(Name = "AutomaticNumber")]
			public string AutomaticNumber = "";

			[DataMember(Name = "InternetMode")]
			public bool InternetMode;

			[DataMember(Name = "BSOMode")]
			public bool BSOMode;

			[DataMember(Name = "ServiceMode")]
			public bool ServiceMode;

			[DataMember(Name = "PrinterAutomatic")]
			public bool PrinterAutomatic;

			[DataMember(Name = "SaleExcisableGoods")]
			public bool SaleExcisableGoods;

			[DataMember(Name = "SignOfGambling")]
			public bool SignOfGambling;

			[DataMember(Name = "SignOfLottery")]
			public bool SignOfLottery;

			[DataMember(Name = "SaleMarking")]
			public bool SaleMarking;

			[DataMember(Name = "SignPawnshop")]
			public bool SignPawnshop;

			[DataMember(Name = "SignAssurance")]
			public bool SignAssurance;

			[DataMember(Name = "SignOfAgent")]
			public string SignOfAgent = "";

			[DataMember(Name = "UrlServerOfd")]
			public string UrlServerOfd = "";

			[DataMember(Name = "PortServerOfd")]
			public string PortServerOfd = "";

			[DataMember(Name = "NameOFD")]
			public string NameOFD = "";

			[DataMember(Name = "UrlOfd")]
			public string UrlOfd = "";

			[DataMember(Name = "InnOfd")]
			public string InnOfd = "";

			[DataMember(Name = "OFD_Error")]
			public string OFD_Error = "";

			[DataMember(Name = "OFD_NumErrorDoc")]
			public int OFD_NumErrorDoc;

			[DataMember(Name = "OFD_DateErrorDoc")]
			public DateTime OFD_DateErrorDoc;

			[DataMember(Name = "KktNumber")]
			public string KktNumber = "";

			[DataMember(Name = "FnNumber")]
			public string FnNumber = "";

			[DataMember(Name = "RegNumber")]
			public string RegNumber = "";

			[DataMember(Name = "FN_IsFiscal")]
			public bool FN_IsFiscal;

			[DataMember(Name = "FN_MemOverflowl")]
			public bool FN_MemOverflowl;

			[DataMember(Name = "FN_DateStart")]
			public DateTime FN_DateStart;

			[DataMember(Name = "FN_DateEnd")]
			public DateTime FN_DateEnd;

			[DataMember(Name = "FFDVersion")]
			public string FFDVersion = "1.0";

			[DataMember(Name = "FFDVersionFN")]
			public string FFDVersionFN = "1.0";

			[DataMember(Name = "FFDVersionKKT")]
			public string FFDVersionKKT = "1.0";

			[DataMember(Name = "SetFfdVersion", EmitDefaultValue = false)]
			public int SetFfdVersion;

			[DataMember(Name = "OnOff")]
			public bool OnOff;

			[DataMember(Name = "Active")]
			public bool Active;

			[DataMember(Name = "SessionState")]
			public int SessionState;

			[DataMember(Name = "PaperOver")]
			public bool PaperOver;

			[DataMember(Name = "BalanceCash")]
			public decimal BalanceCash = -1m;

			[DataMember(Name = "DateTimeKKT")]
			public DateTime DateTimeKKT;

			[DataMember(Name = "Firmware_Version", EmitDefaultValue = false)]
			[DefaultValue("")]
			public string Firmware_Version = "";

			[DataMember(Name = "Firmware_Status")]
			public int Firmware_Status = -1;

			[DataMember(Name = "LicenseExpirationDate")]
			public DateTime LicenseExpirationDate;
		}

		[DataContract]
		public class TypeAgentData
		{
			[DataMember(Name = "PayingAgentOperation")]
			public string PayingAgentOperation = "";

			[DataMember(Name = "PayingAgentPhone")]
			public string PayingAgentPhone = "";

			[DataMember(Name = "ReceivePaymentsOperatorPhone")]
			public string ReceivePaymentsOperatorPhone = "";

			[DataMember(Name = "MoneyTransferOperatorPhone")]
			public string MoneyTransferOperatorPhone = "";

			[DataMember(Name = "MoneyTransferOperatorName")]
			public string MoneyTransferOperatorName = "";

			[DataMember(Name = "MoneyTransferOperatorAddress")]
			public string MoneyTransferOperatorAddress = "";

			[DataMember(Name = "MoneyTransferOperatorVATIN")]
			public string MoneyTransferOperatorVATIN = "";
		}

		[DataContract]
		public class TypePurveyorData
		{
			[DataMember(Name = "PurveyorPhone")]
			public string PurveyorPhone = "";

			[DataMember(Name = "PurveyorName")]
			public string PurveyorName = "";

			[DataMember(Name = "PurveyorVATIN")]
			public string PurveyorVATIN = "";
		}

		[DataContract]
		public class TypeUserAttribute
		{
			[DataMember(Name = "Name")]
			public string Name = "";

			[DataMember(Name = "Value")]
			public string Value = "";
		}

		[DataContract]
		public class CommonAccessZones
		{
			[DataMember(Name = "ZoneId", EmitDefaultValue = false)]
			[DefaultValue("")]
			public string ZoneId = "";

			[DataMember(Name = "ZoneName", EmitDefaultValue = false)]
			[DefaultValue("")]
			public string ZoneName = "";
		}

		[DataMember(Name = "Command")]
		public string Command = "";

		[DataMember(Name = "NumDevice")]
		public int NumDevice;

		[DataMember(Name = "IdDevice", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string IdDevice = "";

		[DataMember(Name = "IP_client", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string IP_client = "";

		[DataMember(Name = "Marke", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string Marke;

		public bool IsGood = true;

		[DataMember(Name = "KktNumber", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string KktNumber = "";

		[DataMember(Name = "IdCommand")]
		public string IdCommand = "";

		public string IdCommandInternal = "";

		[DataMember(Name = "AdditionalActions", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string AdditionalActions = "";

		[DataMember(Name = "IsXML", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool IsXML;

		[DataMember(Name = "XML", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string XML = "";

		[DataMember(Name = "Timeout")]
		public int Timeout;

		public int WaitTimeout;

		[DataMember(Name = "RunAsAddIn", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool RunAsAddIn;

		[DataMember(Name = "UseAddInDialogPrintCheck", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool UseAddInDialogPrintCheck;

		[DataMember(Name = "UseAddInDialogSelectDevice", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool UseAddInDialogSelectDevice;

		[DataMember(Name = "MessageFrom", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string MessageFrom;

		[DataMember(Name = "MessageTo", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string MessageTo;

		[DataMember(Name = "NoError", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool NoError;

		[DataMember(Name = "KeySubLicensing", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string KeySubLicensing = "";

		[DataMember(Name = "UnitPassword", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string UnitPassword = "";

		[DataMember(Name = "InnKkm", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string InnKkm = "";

		[DataMember(Name = "IsFiscalCheck", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool IsFiscalCheck;

		[DataMember(Name = "TypeCheck", EmitDefaultValue = false)]
		[DefaultValue(0)]
		public int TypeCheck;

		[DataMember(Name = "NotPrint", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool? NotPrint = false;

		[DataMember(Name = "NumberCopies", EmitDefaultValue = false)]
		[DefaultValue(0)]
		public int NumberCopies;

		[DataMember(Name = "CancelOpenedCheck", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool CancelOpenedCheck;

		[DataMember(Name = "CheckStrings", EmitDefaultValue = false)]
		public CheckString[] CheckStrings = new CheckString[0];

		[DataMember(Name = "GoodCodeDatas", EmitDefaultValue = false)]
		public List<GoodCodeData> GoodCodeDatas = new List<GoodCodeData>();

		[DataMember(Name = "CashierName", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string CashierName = "";

		[DataMember(Name = "CashierVATIN", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string CashierVATIN = "";

		[DataMember(Name = "ClientAddress", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string ClientAddress = "";

		[DataMember(Name = "ClientInfo", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string ClientInfo = "";

		[DataMember(Name = "ClientINN", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string ClientINN = "";

		[DataMember(Name = "SenderEmail", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string SenderEmail = "";

		[DataMember(Name = "AddressSettle", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string AddressSettle = "";

		[DataMember(Name = "PlaceMarket", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string PlaceMarket = "";

		[DataMember(Name = "InternetMode", EmitDefaultValue = false)]
		[DefaultValue("")]
		public bool? InternetMode;

		[DataMember(Name = "AgentSign", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public int? AgentSign;

		[DataMember(Name = "AgentData", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public TypeAgentData AgentData;

		[DataMember(Name = "PurveyorData", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public TypePurveyorData PurveyorData;

		[DataMember(Name = "UserAttribute", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public TypeUserAttribute UserAttribute;

		[DataMember(Name = "AdditionalAttribute", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string AdditionalAttribute = "";

		[DataMember(Name = "Cash", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal Cash;

		[DataMember(Name = "ElectronicPayment", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal ElectronicPayment;

		[DataMember(Name = "AdvancePayment", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal AdvancePayment;

		[DataMember(Name = "Credit", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal Credit;

		[DataMember(Name = "CashProvision", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal CashProvision;

		[DataMember(Name = "CashLessType1", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal CashLessType1;

		[DataMember(Name = "CashLessType2", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal CashLessType2;

		[DataMember(Name = "CashLessType3", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal CashLessType3;

		[DataMember(Name = "CorrectionBaseDate", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public DateTime? CorrectionBaseDate;

		[DataMember(Name = "CorrectionBaseNumber", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string CorrectionBaseNumber = "";

		[DataMember(Name = "CorrectionType", EmitDefaultValue = false)]
		[DefaultValue(1)]
		public int CorrectionType = 1;

		[DataMember(Name = "SumTaxNone", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTaxNone;

		[DataMember(Name = "SumTax22", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax22;

		[DataMember(Name = "SumTax20", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax20;

		[DataMember(Name = "SumTax18", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax18;

		[DataMember(Name = "SumTax10", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax10;

		[DataMember(Name = "SumTax5", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax5;

		[DataMember(Name = "SumTax7", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax7;

		[DataMember(Name = "SumTax0", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax0;

		[DataMember(Name = "SumTax122", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax122;

		[DataMember(Name = "SumTax120", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax120;

		[DataMember(Name = "SumTax118", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax118;

		[DataMember(Name = "SumTax110", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax110;

		[DataMember(Name = "SumTax105", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax105;

		[DataMember(Name = "SumTax107", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal SumTax107;

		[DataMember(Name = "Amount", EmitDefaultValue = false)]
		[DefaultValue(0.0)]
		public decimal Amount;

		[DataMember(Name = "TaxVariant", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string TaxVariant = "";

		[DataMember(Name = "CheckProps", EmitDefaultValue = false)]
		public CheckProp[] CheckProps = new CheckProp[0];

		[DataMember(Name = "AdditionalProps", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public AdditionalProp[] AdditionalProps;

		[DataMember(Name = "RegKkmOfd", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public TypeRegKkmOfd RegKkmOfd;

		[DataMember(Name = "Sound", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool Sound;

		[DataMember(Name = "SettingsServer", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public Global.SettingsServer SettingsServer;

		[DataMember(Name = "DeviceSettings", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public Global.DeviceSettings DeviceSettings;

		[DataMember(Name = "RezultCommandProcessing", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public RezultCommandProcessing RezultCommandProcessing;

		[DataMember(Name = "CommandMessage", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string CommandMessage = "";

		[DataMember(Name = "UniversalID", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string UniversalID = "";

		[DataMember(Name = "CardNumber", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string CardNumber = "";

		[DataMember(Name = "ReceiptNumber", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string ReceiptNumber = "";

		[DataMember(Name = "RRNCode", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string RRNCode = "";

		[DataMember(Name = "AuthorizationCode", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string AuthorizationCode = "";

		[DataMember(Name = "IdProcessing", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string IdProcessing = "";

		[DataMember(Name = "Detailed", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool Detailed;

		[DataMember(Name = "Department", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public uint? Department;

		[DataMember(Name = "PayByProcessing", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public bool? PayByProcessing;

		[DataMember(Name = "PrintSlipAfterCheck", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public bool? PrintSlipAfterCheck;

		[DataMember(Name = "PrintSlipForCashier", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public bool? PrintSlipForCashier;

		[DataMember(Name = "NumDeviceByProcessing", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public int? NumDeviceByProcessing;

		[DataMember(Name = "OnSBP", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool OnSBP;

		[DataMember(Name = "Active", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public bool? Active;

		[DataMember(Name = "OnOff", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public bool? OnOff;

		[DataMember(Name = "OFD_DateErrorDoc", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public DateTime? OFD_DateErrorDoc;

		[DataMember(Name = "OFD_Error", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public bool? OFD_Error;

		[DataMember(Name = "FN_DateEnd", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public DateTime? FN_DateEnd;

		[DataMember(Name = "FN_MemOverflowl", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public bool? FN_MemOverflowl;

		[DataMember(Name = "FN_IsFiscal", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public bool? FN_IsFiscal;

		[DataMember(Name = "TypeDevice", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string TypeDevice = "";

		[DataMember(Name = "KkmIP", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string KkmIP = "";

		[DataMember(Name = "FiscalNumber", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public int? FiscalNumber;

		[DataMember(Name = "ClearText", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool NotClearText;

		public bool? RunComPort;

		[DataMember(Name = "IdTypeDevice", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string IdTypeDevice = "";

		[DataMember(Name = "Operator", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string Operator;

		[DataMember(Name = "GuestName", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string GuestName;

		[DataMember(Name = "RoomNumbers", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string[] RoomNumbers;

		[DataMember(Name = "IsCopyKey", EmitDefaultValue = false)]
		[DefaultValue(false)]
		public bool IsCopyKey;

		[DataMember(Name = "DateStart", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public DateTime? DateStart;

		[DataMember(Name = "DateEnd", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public DateTime? DateEnd;

		[DataMember(Name = "CommonAccessZonesId", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string[] CommonAccessZonesId;

		[DataMember(Name = "BarCode", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string BarCode;

		[DataMember(Name = "TopString", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string TopString;

		[DataMember(Name = "BottomString", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string BottomString;

		[DataMember(Name = "CodeQR", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string CodeQR;

		[JsonConstructor]
		public DataCommand()
		{
		}
	}

	[DataContract]
	public class RezultCommand
	{
		[DataMember(Name = "Command")]
		public string Command = "";

		[DataMember(Name = "Error")]
		public string Error = "";

		[DataMember(Name = "Warning")]
		public string Warning = "";

		[DataMember(Name = "Message")]
		public string Message = "";

		[DataMember(Name = "Status")]
		public ExecuteStatus Status;

		[DataMember(Name = "IdCommand")]
		public string IdCommand = "";

		[DataMember(Name = "NumDevice", EmitDefaultValue = false)]
		[DefaultValue(0)]
		public int NumDevice;

		[DataMember(Name = "UnitName", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string UnitName = "";

		public bool CommandEnd;

		[DataMember(Name = "Url", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string Url;

		[DataMember(Name = "LoginAdmin", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string LoginAdmin;

		[DataMember(Name = "PassAdmin", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string PassAdmin;

		[DataMember(Name = "Verson", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string Verson;

		[DataMember(Name = "List", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public MenegerServer.RezultCommandList List;

		[DataMember(Name = "RunAsAddIn", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public bool? RunAsAddIn;

		public bool? RunComPort;

		public RezultCommand SubRezultCommand;

		[DataMember(Name = "MessageHTML", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string MessageHTML;

		[DataMember(Name = "TypeMessageHTM", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string TypeMessageHTM;

		[DataMember(Name = "MessageFrom", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string MessageFrom;

		[DataMember(Name = "MessageTo", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string MessageTo;

		[DataMember(Name = "KeySubLicensing", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string KeySubLicensing = "";
	}

	[DataContract]
	public class RezultCommandKKm : RezultCommand
	{
		[DataContract]
		public class tMarkingCodeValidation
		{
			[DataContract]
			public class TValidationPR
			{
				[DataMember(Name = "ValidationResult")]
				public bool ValidationResult;

				[DataMember(Name = "ValidationDisabled")]
				public bool ValidationDisabled;

				[DataMember(Name = "DecryptionResult")]
				public string DecryptionResult = "";

				[DataMember(Name = "Log")]
				public PermitRegim.TRezult.TCode? Log;
			}

			[DataContract]
			public class TValidationKKT
			{
				[DataMember(Name = "ValidationResult")]
				public uint ValidationResult;

				[DataMember(Name = "DecryptionResult")]
				public string DecryptionResult = "";
			}

			[DataMember(Name = "Name")]
			[DefaultValue(null)]
			public string Name;

			[DataMember(Name = "BarCode")]
			public string BarCode;

			[DataMember(Name = "IndustryProps")]
			[DefaultValue(null)]
			public string IndustryProps;

			[DataMember(Name = "ValidationPR")]
			public TValidationPR ValidationPR = new TValidationPR();

			[DataMember(Name = "ValidationKKT")]
			public TValidationKKT ValidationKKT = new TValidationKKT();
		}

		[DataMember(Name = "CheckNumber", EmitDefaultValue = false)]
		[DefaultValue(0)]
		public long CheckNumber;

		[DataMember(Name = "SessionNumber", EmitDefaultValue = false)]
		[DefaultValue(0)]
		public int SessionNumber;

		[DataMember(Name = "SessionCheckNumber", EmitDefaultValue = false)]
		[DefaultValue(0)]
		public int SessionCheckNumber;

		[DataMember(Name = "LineLength", EmitDefaultValue = false)]
		[DefaultValue(-1)]
		public int LineLength = -1;

		[DataMember(Name = "URL", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string URL = "";

		[DataMember(Name = "QRCode", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string QRCode = "";

		[DataMember(Name = "Info", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public DataCommand.TypeRegKkmOfd Info;

		[DataMember(Name = "Cash", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public decimal? Cash;

		[DataMember(Name = "ElectronicPayment", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public decimal? ElectronicPayment;

		[DataMember(Name = "AdvancePayment", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public decimal? AdvancePayment;

		[DataMember(Name = "Credit", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public decimal? Credit;

		[DataMember(Name = "CashProvision", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public decimal? CashProvision;

		[DataMember(Name = "MarkingCodeValidation", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public List<tMarkingCodeValidation> MarkingCodeValidation;

		[DataMember(Name = "RezultProcessing", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public RezultCommandProcessing RezultProcessing;
	}

	[DataContract]
	public class RezultMarkingCodeValidation : RezultCommand
	{
		[DataContract]
		public class tMarkingCodeValidation
		{
			[DataContract]
			public class TValidationPR
			{
				[DataMember(Name = "ValidationResult")]
				public bool ValidationResult;

				[DataMember(Name = "ValidationDisabled")]
				public bool ValidationDisabled;

				[DataMember(Name = "DecryptionResult")]
				public string DecryptionResult = "";

				[DataMember(Name = "Result")]
				public PermitRegim.TRezult.TCode? Result;
			}

			[DataContract]
			public class TValidationKKT
			{
				[DataMember(Name = "ValidationResult")]
				public uint ValidationResult;

				[DataMember(Name = "DecryptionResult")]
				public string DecryptionResult = "";
			}

			[DataMember(Name = "Name")]
			[DefaultValue(null)]
			public string Name;

			[DataMember(Name = "BarCode")]
			public string BarCode;

			public string TryBarCode;

			[DataMember(Name = "IndustryProps")]
			[DefaultValue(null)]
			public string IndustryProps;

			[DataMember(Name = "ValidationPR")]
			public TValidationPR ValidationPR = new TValidationPR();

			[DataMember(Name = "ValidationKKT")]
			public TValidationKKT ValidationKKT = new TValidationKKT();
		}

		[DataMember(Name = "MarkingCodeValidation", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public List<tMarkingCodeValidation> MarkingCodeValidation = new List<tMarkingCodeValidation>();
	}

	[DataContract]
	public class RezultCounters : RezultCommand
	{
		[DataContract]
		public class tСounter
		{
			[DataMember(Name = "CountersType")]
			public string CountersType = "";

			[DataMember(Name = "ReceiptType")]
			public string ReceiptType = "";

			[DataMember(Name = "Count")]
			public uint Count;

			[DataMember(Name = "Sum")]
			public decimal Sum;

			[DataMember(Name = "Cash")]
			public decimal Cash;

			[DataMember(Name = "ElectronicPayment")]
			public decimal ElectronicPayment;

			[DataMember(Name = "AdvancePayment")]
			public decimal AdvancePayment;

			[DataMember(Name = "Credit")]
			public decimal Credit;

			[DataMember(Name = "CashProvision")]
			public decimal CashProvision;

			[DataMember(Name = "Tax22")]
			public decimal Tax22;

			[DataMember(Name = "Tax10")]
			public decimal Tax10;

			[DataMember(Name = "Tax0")]
			public decimal Tax0;

			[DataMember(Name = "TaxNo")]
			public decimal TaxNo;

			[DataMember(Name = "Tax122")]
			public decimal Tax122;

			[DataMember(Name = "Tax110")]
			public decimal Tax110;

			[DataMember(Name = "CorrectionsCount")]
			public uint CorrectionsCount;

			[DataMember(Name = "CorrectionsSum")]
			public decimal CorrectionsSum;
		}

		[DataMember(Name = "Counters", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public List<tСounter> Counters = new List<tСounter>();
	}

	[DataContract]
	public class RezultCommandCheck : RezultCommand
	{
		[DataContract]
		public class Register
		{
			[DataMember(Name = "Name")]
			public string Name = "";

			[DataMember(Name = "Quantity")]
			public decimal Quantity;

			[DataMember(Name = "Amount")]
			public decimal Amount;

			[DataMember(Name = "Tax")]
			public decimal Tax;

			[DataMember(Name = "GoodCodeData", EmitDefaultValue = false)]
			public DataCommand.Register.tGoodCodeData GoodCodeData;
		}

		[DataContract]
		public class RegisterCheck
		{
			[DataMember(Name = "FiscalNumber")]
			public string FiscalNumber = "";

			[DataMember(Name = "FiscalDate")]
			public DateTime FiscalDate;

			[DataMember(Name = "CheckType")]
			public string CheckType;

			[DataMember(Name = "FiscalSign")]
			public string FiscalSign;

			[DataMember(Name = "CashierName")]
			public string CashierName = "";

			[DataMember(Name = "CashierVATIN")]
			public string CashierVATIN = "";

			[DataMember(Name = "TaxVariant")]
			public string TaxVariant = "";

			[DataMember(Name = "ClientAddress")]
			public string ClientAddress = "";

			[DataMember(Name = "SenderEmail")]
			public string SenderEmail = "";

			[DataMember(Name = "PlaceMarket")]
			public string PlaceMarket = "";

			[DataMember(Name = "Register")]
			public Register[] Register;

			[DataMember(Name = "Cash")]
			public decimal Cash;

			[DataMember(Name = "ElectronicPayment")]
			public decimal ElectronicPayment;

			[DataMember(Name = "AdvancePayment")]
			public decimal AdvancePayment;

			[DataMember(Name = "Credit")]
			public decimal Credit;

			[DataMember(Name = "CashProvision")]
			public decimal CashProvision;

			[DataMember(Name = "AllSumm")]
			public decimal AllSumm;
		}

		[DataMember(Name = "Slip", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string Slip;

		[DataMember(Name = "RegisterCheck", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public RegisterCheck Check;

		[DataMember(Name = "URL", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string URL = "";

		[DataMember(Name = "QRCode", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string QRCode = "";
	}

	[DataContract]
	public class RezultCommandBarCode : RezultCommand
	{
		[DataContract]
		public class UnitEvent
		{
			[DataMember(Name = "Source")]
			public string Source = "";

			[DataMember(Name = "Message")]
			public string Message = "";

			[DataMember(Name = "Data")]
			public string Data = "";
		}

		[DataMember(Name = "Event")]
		public UnitEvent Event;
	}

	public class RezultCommandLibra : RezultCommand
	{
		[DataMember(Name = "Weight")]
		public decimal Weight;
	}

	public class RezultCommandProcessing : RezultCommand
	{
		[DataMember(Name = "UniversalID")]
		public string UniversalID = "";

		[DataMember(Name = "Amount")]
		public decimal Amount;

		[DataMember(Name = "CardNumber")]
		public string CardNumber = "";

		[DataMember(Name = "ReceiptNumber")]
		public string ReceiptNumber = "";

		[DataMember(Name = "RRNCode")]
		public string RRNCode = "";

		[DataMember(Name = "AuthorizationCode")]
		public string AuthorizationCode = "";

		[DataMember(Name = "IdProcessing")]
		public string IdProcessing = "";

		[DataMember(Name = "Slip")]
		public string Slip = "";

		[DataMember(Name = "PrintSlipOnTerminal")]
		public bool PrintSlipOnTerminal;

		[DataMember(Name = "CardHash", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string CardHash = "";

		[DataMember(Name = "CardDPAN", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string CardDPAN = "";

		[DataMember(Name = "CardEncryptedData", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string CardEncryptedData = "";

		[DataMember(Name = "TransDate", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public DateTime? TransDate;

		[DataMember(Name = "TerminalID", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string TerminalID;
	}

	public class RezultCommandLocks : RezultCommand
	{
		[DataContract]
		public class TLocksInfo
		{
			[DataMember(Name = "LocksSystem", EmitDefaultValue = false)]
			[DefaultValue("")]
			public string LocksSystem = "";

			[DataMember(Name = "CountRoomsInkey", EmitDefaultValue = false)]
			[DefaultValue(0)]
			public int CountRoomsInkey;

			[DataMember(Name = "CountDigitsInRoomNumber", EmitDefaultValue = false)]
			[DefaultValue(0)]
			public int CountDigitsInRoomNumber;

			[DataMember(Name = "CommonAccessZones", EmitDefaultValue = false)]
			[DefaultValue(null)]
			public List<DataCommand.CommonAccessZones> CommonAccessZones;
		}

		[DataMember(Name = "LocksInfo", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public TLocksInfo LocksInfo;

		[DataMember(Name = "RoomNumbers", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public List<string> RoomNumbers;

		[DataMember(Name = "DateStart", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public DateTime? DateStart;

		[DataMember(Name = "DateEnd", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public DateTime? DateEnd;

		[DataMember(Name = "CommonAccessZonesId", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public List<string> CommonAccessZonesId;

		[DataMember(Name = "Operator", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string Operator = "";

		[DataMember(Name = "GuestName", EmitDefaultValue = false)]
		[DefaultValue("")]
		public string GuestName = "";
	}

	public class RezultCommandGetRezult : RezultCommand
	{
		[DataMember(Name = "Rezult")]
		public RezultCommand Rezult;
	}

	public class RezultCommandGetTypeDevice : RezultCommand
	{
		[DataContract]
		public class iTypesDevice
		{
			[DataMember(Name = "Id")]
			public string Id;

			[DataMember(Name = "Type")]
			public int Type;

			[DataMember(Name = "Protocol")]
			public string Protocol;

			[DataMember(Name = "SupportModels")]
			public string SupportModels;
		}

		[DataContract]
		public class iTypeDevices
		{
		}

		[DataContract]
		public class iUnitParamets
		{
			[DataMember(Name = "NumDevice")]
			public int NumDevice;

			[DataMember(Name = "Id")]
			public string Id = "";

			[DataMember(Name = "Paramets")]
			public Dictionary<string, string> Paramets = new Dictionary<string, string>();
		}

		[DataContract]
		public class iSettingsServer
		{
			[DataMember]
			public int ipPort;

			[DataMember]
			public string LoginAdmin = "Admin";

			[DataMember]
			public string PassAdmin = "";

			[DataMember(Name = "LoginUser")]
			public string LoginUser = "User";

			[DataMember]
			public string PassUser = "";

			[DataMember]
			public string ServerSertificate = "";

			[DataMember]
			public string TypeRun = "Windows";

			[DataMember]
			public bool RegisterAllCommand;

			[DataMember]
			public int RemoveCommandInterval = 30;

			[DataMember]
			public bool SetNotActiveOnError;

			[DataMember]
			public bool SetNotActiveOnPaperOver;

			[DataMember]
			public bool KkmIniter;

			[DataMember]
			public int KkmIniterInterval = 5;
		}

		[DataMember(Name = "Types", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public Dictionary<int, string> Types;

		[DataMember(Name = "TypesDevices", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public List<iTypesDevice> TypesDevices;

		[DataMember(Name = "TypeDevice")]
		public TypeDevice TypeDevice;

		[DataMember(Name = "Paramets")]
		public List<iUnitSettings.Paramert> Paramets;

		[DataMember(Name = "UnitParamets", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public iUnitParamets UnitParamets;

		[DataMember(Name = "UnitSettings", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public iUnitSettings UnitSettings;

		[DataMember(Name = "SettingsServer", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public iSettingsServer SettingsServer;
	}

	public class RezultCommandUseAddInDialog : RezultCommand
	{
		[DataMember(Name = "UseAddInDialogHTML", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string UseAddInDialogHTML;

		[DataMember(Name = "UseAddInDialogScript", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public string UseAddInDialogScript;

		[DataMember(Name = "RezultCommand", EmitDefaultValue = false)]
		[DefaultValue(null)]
		public RezultCommand RezultCommand;
	}

	public class RezultCommandCD : RezultCommand
	{
		[DataMember(Name = "IsTopString", EmitDefaultValue = true)]
		[DefaultValue(false)]
		public bool IsTopString;

		[DataMember(Name = "IsBottomString", EmitDefaultValue = true)]
		[DefaultValue(false)]
		public bool IsBottomString;

		[DataMember(Name = "IsCodeQR", EmitDefaultValue = true)]
		[DefaultValue(false)]
		public bool IsCodeQR;
	}

	public enum ExecuteStatus
	{
		Ok,
		Run,
		Error,
		NotFound,
		NotRun,
		AlreadyDone
	}

	public class SetKkm
	{
		public string NumberKkm = "";

		public string INN = "";

		public string Organization = "<Не определено>";

		public string TaxVariant = "";

		public int PrintingWidth;

		public bool IsKKT;

		public bool PaperOver;

		public string OFD_Error = "";

		public int OFD_NumErrorDoc;

		public DateTime OFD_DateErrorDoc;

		public string Fn_Number = "";

		public DateTime FN_DateStart;

		public DateTime FN_DateEnd;

		public bool FN_IsFiscal;

		public bool FN_MemOverflowl;

		public string UrlServerOfd = "";

		public string PortServerOfd = "";

		public string NameOFD = "";

		public string UrlOfd = "";

		public string InnOfd = "";

		public string UrlTax = "";

		public string RegNumber = "";

		public string AddressSettle = "";

		public string PlaceSettle = "";

		public string SenderEmail = "";

		public bool EncryptionMode;

		public bool OfflineMode;

		public bool AutomaticMode;

		public string AutomaticNumber = "";

		public bool InternetMode;

		public bool BSOMode;

		public bool ServiceMode;

		public bool PrinterAutomatic;

		public bool SignOfGambling;

		public bool SignOfLottery;

		public bool SaleExcisableGoods;

		public bool SaleMarking;

		public bool SignPawnshop;

		public bool SignAssurance;

		public string SignOfAgent = "";

		public byte FN_Status;

		public string InfoRegKkt = "";

		public byte FfdVersion = 2;

		public byte FfdSupportVersion = 2;

		public byte FfdMinimumVersion = 2;

		public string Firmware_Version = "<Не определено>";

		public int Firmware_Status = -1;

		public DateTime DateTimeKKT;

		public int EmulationCheckNum;
	}

	public class ClPortLogs
	{
		private Unit Unut;

		public int Status;

		public Stopwatch Stopwatch = new Stopwatch();

		public ClPortLogs(Unit Unut)
		{
			this.Unut = Unut;
		}

		public void Write(byte[] Data)
		{
			if (Status != 1)
			{
				Status = 1;
				TimeSpan elapsed = Stopwatch.Elapsed;
				Unut.NetLogs.Append("\r\n< " + elapsed.ToString("mm\\:ss\\.ffffff") + ": ");
				Stopwatch.Restart();
			}
			else
			{
				Unut.NetLogs.Append("-");
			}
			Unut.NetLogs.Append(BitConverter.ToString(Data));
		}

		public void Write(string Data)
		{
			if (Status != 1)
			{
				Status = 1;
				TimeSpan elapsed = Stopwatch.Elapsed;
				Unut.NetLogs.Append("\r\n< " + elapsed.ToString("mm\\:ss\\.ffffff") + ": ");
				Stopwatch.Restart();
			}
			else
			{
				Unut.NetLogs.Append("-");
			}
			Unut.NetLogs.Append(Data);
		}

		public void Read(byte[] Data)
		{
			if (Status != 2)
			{
				Status = 2;
				TimeSpan elapsed = Stopwatch.Elapsed;
				Unut.NetLogs.Append("\r\n> " + elapsed.ToString("mm\\:ss\\.ffffff") + ": ");
				Stopwatch.Restart();
			}
			else
			{
				Unut.NetLogs.Append("-");
			}
			Unut.NetLogs.Append(BitConverter.ToString(Data));
		}

		public void Read(string Data)
		{
			if (Status != 2)
			{
				Status = 2;
				TimeSpan elapsed = Stopwatch.Elapsed;
				Unut.NetLogs.Append("\r\n> " + elapsed.ToString("mm\\:ss\\.ffffff") + ": ");
				Stopwatch.Restart();
			}
			else
			{
				Unut.NetLogs.Append("-");
			}
			Unut.NetLogs.Append(Data);
		}

		public void Append(string Data, string Znack = ">", bool CleatStatus = true)
		{
			if (CleatStatus)
			{
				Status = 0;
			}
			TimeSpan elapsed = Stopwatch.Elapsed;
			Unut.NetLogs.Append("\r\n" + Znack + " " + elapsed.ToString("mm\\:ss\\.ffffff") + ": " + Data);
			Stopwatch.Restart();
		}

		public void AppendText(string Data)
		{
			Unut.NetLogs.Append("\r\n" + Data);
		}
	}

	public class UnitEventArgs : EventArgs
	{
		public string Message { get; set; }

		public object Object { get; set; }

		public UnitEventArgs(string message, object obj)
		{
			Message = message;
			Object = obj;
		}
	}

	public SemaphoreSlim Semaphore = new SemaphoreSlim(1);

	public string IdDll = "";

	public Global.DeviceSettings SettDr;

	public string UnitVersion = "";

	public string UnitName = "";

	public string UnitDescription = "";

	public string UnitAdditionallinks = "";

	public string UnitEquipmentType = "";

	public double UnitInterfaceRevision;

	public bool UnitIntegrationLibrary;

	public bool UnitMainDriverInstalled;

	public string UnitDownloadURL = "";

	public string LastError = "";

	public string Error = "";

	public string Warning = "";

	public string InfoUnit = "";

	public string DemoModeIsActivated = "";

	public bool IsOldDriver;

	public int NumUnit = -1;

	public int UnitOpen;

	public bool Active = true;

	public bool IsInit;

	public bool IsInitOfd;

	public bool Block;

	public DateTime IsInitDate = DateTime.Now;

	public DateTime IsFullInitDate = DateTime.Now;

	public DateTime LastCommandDate = DateTime.Now;

	public DateTime StartCommandDate;

	private const int ConstPeriodWorkCommand = 120;

	public int PeriodWorkCommand = 120;

	public int SessionOpen = 2;

	public bool IsNotErrorStatus;

	public bool IsNotSetOrStatus;

	public bool UseBuiltTerminal;

	public ComDevice.PaymentOption LicenseFlags;

	public string NameDevice = "<Не определено>";

	public int IdModel;

	public SetKkm Kkm = new SetKkm();

	public iUnitSettings UnitSettings = new iUnitSettings();

	public Dictionary<string, string> UnitParamets = new Dictionary<string, string>();

	public Dictionary<string, string> UnitActions = new Dictionary<string, string>();

	public string DeviceID = "";

	public StringBuilder NetLogs = new StringBuilder();

	private DateTime TimeStart = DateTime.Now;

	public List<Global.TextLine> TextLines = new List<Global.TextLine>();

	public bool IsCommandCancelled;

	public DataCommand CurDataCommand;

	public RezultCommand CurRezultCommand;

	public bool CancellationCommand;

	public bool SupportsSBP;

	protected string IndustryProps1262 = "030";

	protected string IndustryProps1263 = "21.11.2023";

	protected string IndustryProps1264 = "1944";

	protected string PharmaProps1262 = "020";

	protected string PharmaProps1263 = "14.12.2018";

	protected string PharmaProps1264 = "1556";

	protected string HorecaProps1262 = "030";

	protected string HorecaProps1263 = "26.03.2022";

	protected string HorecaProps1264 = "477";

	public ClPortLogs PortLogs;

	private static string DialogTrackingStatusHtml;

	private static string DialogSelectDeviceHtml;

	private static string DialogSelectDeviceJs;

	private static string DialogPrintCheckHtml;

	private static string DialogPrintCheckJs;

	public event EventHandler<UnitEventArgs> UnitEvents;

	public Unit()
	{
	}

	public Unit(Global.DeviceSettings SettDr, int NumUnit)
	{
		PortLogs = new ClPortLogs(this);
		this.SettDr = SettDr;
		this.NumUnit = NumUnit;
		if (SettDr != null)
		{
			Active = SettDr.Active;
		}
	}

	public virtual void Destroy()
	{
	}

	public virtual void InitDll()
	{
	}

	public virtual void LoadParamets()
	{
		UnitName = "Устройство не инициализировано!";
		UnitDescription = Error ?? "";
	}

	public virtual void WriteParametsToUnits()
	{
		if (UnitParamets.ContainsKey("NameDevice"))
		{
			UnitName = UnitParamets["NameDevice"];
		}
	}

	public virtual async Task<bool> InitDevice(bool FullInit = false, bool Program = false)
	{
		LastError = "";
		if (FullInit && Program)
		{
			try
			{
				LoadParamets();
				LoadParametsFromSettings(SettDr);
				WriteParametsToUnits();
			}
			catch (Exception)
			{
				LastError = "Ошибка инициализации драйвера #" + NumUnit + " : " + LastError;
			}
		}
		return true;
	}

	public async Task<bool> ProcessInitDevice(bool FullInit = false, bool Program = false)
	{
		bool Rez = await InitDevice(FullInit, Program);
		if (SettDr.Paramets.ContainsKey("EmulationCheckForm") && SettDr.Paramets["EmulationCheckForm"] == "")
		{
			SettDr.Paramets["EmulationCheckForm"] = GetEmulationCheckForm();
		}
		if (SettDr.Paramets.ContainsKey("EmulationCheck") && SettDr.Paramets["EmulationCheck"].AsBool() && SettDr.Paramets.ContainsKey("RouteCommand") && SettDr.Paramets["RouteCommand"] != "")
		{
			try
			{
				DataCommand dataCommand = new DataCommand();
				dataCommand.Command = "GetDataKKT";
				dataCommand.IdCommand = Guid.NewGuid().ToString();
				await ProcessGetDataKKT(dataCommand, new RezultCommandKKm());
			}
			catch (Exception ex)
			{
				IsInit = false;
				Error = "Ошибка получения данных удаленной ККТ: " + ex.Message;
			}
		}
		return Rez;
	}

	public virtual void SaveParametrs(Dictionary<string, string> NewParamets)
	{
	}

	public virtual async Task ExecuteCommand(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		StartCommandDate = DateTime.Now;
		IsNotErrorStatus = false;
		Warning = "";
		NetLogs.Clear();
		while (Block)
		{
			await Task.Delay(100);
		}
		if (!Active)
		{
			RezultCommand.Error = LastError ?? "";
			await PortCloseAsync();
			return;
		}
		PeriodWorkCommand = 120;
		if (DataCommand.Timeout > PeriodWorkCommand)
		{
			PeriodWorkCommand = DataCommand.Timeout;
		}
		Error = "";
		LastCommandDate = DateTime.Now;
		if (UnitParamets.ContainsKey("UnitPassword") && UnitParamets["UnitPassword"] != "" && UnitParamets["UnitPassword"] != DataCommand.UnitPassword && DataCommand.UnitPassword != "AllUnitsPasswordKkmServer" && DataCommand.Command != "InitDevice" && DataCommand.Command != "DoAdditionalAction" && DataCommand.Command != "DeviceTest")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error = "Не правильно указан пароль на устройство";
			await PortCloseAsync();
			return;
		}
		TimeStart = DateTime.Now;
		CurDataCommand = DataCommand;
		CurRezultCommand = RezultCommand;
		TextLines.Clear();
		CancellationCommand = false;
		WindowTrackingStatus(DataCommand, this, "Выполняется..");
		try
		{
			switch (DataCommand.Command)
			{
			case "InitDevice":
				LastError = "Идет инициализация....";
				if (await ProcessInitDevice(FullInit: true))
				{
					RezultCommand.Status = ExecuteStatus.Ok;
				}
				else
				{
					RezultCommand.Status = ExecuteStatus.Error;
				}
				break;
			case "RegisterCheck":
				await ProcessRegisterCheck(DataCommand, (RezultCommandKKm)RezultCommand);
				LastError = Error;
				break;
			case "ValidationMarkingCode":
				await ProcessValidationMarkingCode(DataCommand, (RezultMarkingCodeValidation)RezultCommand);
				LastError = Error;
				break;
			case "OpenShift":
				await ProcessOpenShift(DataCommand, (RezultCommandKKm)RezultCommand);
				if ((RezultCommand.Status == ExecuteStatus.Error || RezultCommand.Error != "" || Error != "") && UnitParamets["NoErrorOnOpenCloseShift"].AsBool())
				{
					RezultCommand.Error = "";
					Error = "";
					RezultCommand.Status = ExecuteStatus.Ok;
				}
				break;
			case "CloseShift":
				await PaymentCashAndCloseShift(DataCommand, (RezultCommandKKm)RezultCommand);
				if ((RezultCommand.Status == ExecuteStatus.Error || RezultCommand.Error != "" || Error != "") && UnitParamets["NoErrorOnOpenCloseShift"].AsBool())
				{
					RezultCommand.Error = "";
					Error = "";
					RezultCommand.Status = ExecuteStatus.Ok;
				}
				break;
			case "ZReport":
				await PaymentCashAndCloseShift(DataCommand, (RezultCommandKKm)RezultCommand);
				if ((RezultCommand.Status == ExecuteStatus.Error || RezultCommand.Error != "" || Error != "") && UnitParamets["NoErrorOnOpenCloseShift"].AsBool())
				{
					RezultCommand.Error = "";
					Error = "";
					RezultCommand.Status = ExecuteStatus.Ok;
				}
				break;
			case "XReport":
				await ProcessXReport(DataCommand, (RezultCommandKKm)RezultCommand);
				break;
			case "OfdReport":
				await ProcessOfdReport(DataCommand, (RezultCommandKKm)RezultCommand);
				break;
			case "OpenCashDrawer":
				await OpenCashDrawer(DataCommand, (RezultCommandKKm)RezultCommand);
				break;
			case "DepositingCash":
				await ProcessDepositingCash(DataCommand, (RezultCommandKKm)RezultCommand);
				break;
			case "PaymentCash":
				await ProcessPaymentCash(DataCommand, (RezultCommandKKm)RezultCommand);
				break;
			case "GetLineLength":
				await GetLineLength(DataCommand, (RezultCommandKKm)RezultCommand);
				break;
			case "GetDataCheck":
				await ProcessGetDataCheck(DataCommand, (RezultCommandCheck)RezultCommand);
				break;
			case "KkmRegOfd":
				await ProcessKkmRegOfd(DataCommand, (RezultCommandKKm)RezultCommand);
				break;
			case "GetDataKKT":
				await ProcessGetDataKKT(DataCommand, (RezultCommandKKm)RezultCommand);
				break;
			case "GetCounters":
				await ProcessGetCounters(DataCommand, (RezultCounters)RezultCommand);
				break;
			case "PrintDocument":
				await PrintDocument(DataCommand, (RezultCommandKKm)RezultCommand);
				break;
			case "PrintLineLength":
				await GetLineLength(DataCommand, (RezultCommandKKm)RezultCommand);
				break;
			case "DoAdditionalAction":
				DoAdditionalAction(DataCommand, ref RezultCommand);
				break;
			case "OutputOnCustomerDisplay":
				await OutputOnCustomerDisplay(DataCommand, RezultCommand);
				break;
			case "ClearCustomerDisplay":
				await ClearCustomerDisplay(DataCommand, RezultCommand);
				break;
			case "OptionsCustomerDisplay":
				await OptionsCustomerDisplay(DataCommand, (RezultCommandCD)RezultCommand);
				break;
			case "GetBarcode":
				await GetBarcode(DataCommand, (RezultCommandBarCode)RezultCommand);
				break;
			case "OpenBarcode":
				await OpenBarcode(DataCommand, (RezultCommandBarCode)RezultCommand);
				break;
			case "CloseBarcode":
				await CloseBarcode(DataCommand, (RezultCommandBarCode)RezultCommand);
				break;
			case "Calibrate":
				await Calibrate(DataCommand, (RezultCommandLibra)RezultCommand);
				break;
			case "GetWeight":
				await GetWeight(DataCommand, (RezultCommandLibra)RezultCommand);
				break;
			case "PayByPaymentCard":
				await ProcessCommandPayTerminal(DataCommand, (RezultCommandProcessing)RezultCommand, 0);
				break;
			case "ReturnPaymentByPaymentCard":
				await ProcessCommandPayTerminal(DataCommand, (RezultCommandProcessing)RezultCommand, 1);
				break;
			case "CancelPaymentByPaymentCard":
				await ProcessCommandPayTerminal(DataCommand, (RezultCommandProcessing)RezultCommand, 2);
				break;
			case "AuthorisationByPaymentCard":
				await ProcessCommandPayTerminal(DataCommand, (RezultCommandProcessing)RezultCommand, 3);
				break;
			case "AuthConfirmationByPaymentCard":
				await ProcessCommandPayTerminal(DataCommand, (RezultCommandProcessing)RezultCommand, 4);
				break;
			case "CancelAuthorisationByPaymentCard":
				await ProcessCommandPayTerminal(DataCommand, (RezultCommandProcessing)RezultCommand, 5);
				break;
			case "EmergencyReversal":
				await ProcessEmergencyReversal(DataCommand, (RezultCommandProcessing)RezultCommand);
				break;
			case "Settlement":
				await ProcessSettlement(DataCommand, (RezultCommandProcessing)RezultCommand);
				break;
			case "TerminalReport":
				await ProcessTerminalReport(DataCommand, (RezultCommandProcessing)RezultCommand);
				break;
			case "TransactionDetails":
				await ProcessTransactionDetails(DataCommand, (RezultCommandProcessing)RezultCommand);
				break;
			case "PrintSlipOnTerminal":
				await PrintSlipOnTerminal(DataCommand, (RezultCommandProcessing)RezultCommand);
				break;
			default:
				throw new ArgumentException($"Неопознанная команда {DataCommand.Command}");
			}
		}
		catch (Exception ex)
		{
			RezultCommand.Status = ExecuteStatus.Error;
			if (Error == "" || Error == null)
			{
				Error = ((Error == "") ? "" : (Error + ", ")) + Global.GetErrorMessagee(ex);
			}
			RezultCommand.Error += Error;
			try
			{
				if (DataCommand.Command != "GetBarcode" && DataCommand.Command != "OpenBarcode")
				{
					await ((UnitPort)this).PortCloseAsync();
				}
			}
			catch
			{
			}
		}
		if (DataCommand.Command == "InitDevice")
		{
			LastError = Error;
		}
		if (Error != "")
		{
			RezultCommand.Status = ExecuteStatus.Error;
			RezultCommand.Error += Error;
		}
		RezultCommand.Warning = Warning;
		if (IsNotErrorStatus)
		{
			RezultCommand.Status = ExecuteStatus.Ok;
		}
		try
		{
			RezultCommand.UnitName = UnitParamets["NameDevice"];
		}
		catch
		{
			RezultCommand.UnitName = UnitName;
		}
		if (RezultCommand.GetType() == typeof(RezultCommandKKm) && ((RezultCommandKKm)RezultCommand).QRCode != "")
		{
			((RezultCommandKKm)RezultCommand).URL = GetUrlFromQRCode(((RezultCommandKKm)RezultCommand).QRCode, Kkm.InnOfd, Kkm.INN, Kkm.RegNumber);
			if (Global.Verson.Split('.')[1] == "0")
			{
				((RezultCommandKKm)RezultCommand).URL = ((RezultCommandKKm)RezultCommand).QRCode;
			}
		}
		if (DataCommand.Command != "GetBarcode" && DataCommand.Command != "OpenBarcode")
		{
			await PortCloseAsync();
		}
		CurDataCommand = null;
		CurRezultCommand = null;
		UpdateSettingsServer();
		StartCommandDate = default(DateTime);
		TimeSpan timeSpan = DateTime.Now - TimeStart;
		NetLogs.Append("\r\nВремя выполнения команды:" + timeSpan.ToString("mm\\:ss\\.ffffff"));
		RezultCommand.CommandEnd = true;
		CancellationCommand = false;
	}

	public virtual void DeviceTest(DataCommand DataCommand, RezultCommand RezultCommand)
	{
	}

	public virtual void DoAdditionalAction(DataCommand DataCommand, ref RezultCommand RezultCommand)
	{
		if (DataCommand.AdditionalActions == "ClearCheckForm")
		{
			SettDr.Paramets["EmulationCheckForm"] = GetEmulationCheckForm();
			UnitParamets["EmulationCheckForm"] = GetEmulationCheckForm();
		}
	}

	public virtual object GetAhtungData()
	{
		return null;
	}

	public virtual void SetAhtungData(object Data)
	{
	}

	public virtual void Test()
	{
	}

	public void OnUnitEvents(string Message, object Object = null)
	{
		if (this.UnitEvents != null)
		{
			UnitEventArgs e = new UnitEventArgs(Message, Object);
			this.UnitEvents(this, e);
		}
	}

	public virtual async Task RegisterCheck(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
		{
			await PrintDocument(DataCommand, RezultCommand);
		}
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public static bool NeedPayByProcessing(DataCommand DataCommand, Unit Unit)
	{
		if ((!Unit.UseBuiltTerminal || (Unit.UseBuiltTerminal && !Unit.UnitParamets["UseBuiltTerminal"].AsBool())) && !DataCommand.PayByProcessing.HasValue && DataCommand.IsFiscalCheck && DataCommand.ElectronicPayment != 0m && (DataCommand.TypeCheck == 0 || DataCommand.TypeCheck == 1) && Unit.SettDr.Paramets["PayByProcessing"] != "" && Unit.SettDr.Paramets["NumDeviceByProcessing"] != Unit.SettDr.NumDevice.ToString())
		{
			DataCommand.PayByProcessing = true;
		}
		if ((!Unit.UseBuiltTerminal || (Unit.UseBuiltTerminal && !Unit.UnitParamets["UseBuiltTerminal"].AsBool())) && DataCommand.PayByProcessing == true && DataCommand.IsFiscalCheck && DataCommand.ElectronicPayment != 0m && (DataCommand.TypeCheck == 0 || DataCommand.TypeCheck == 1))
		{
			return true;
		}
		return false;
	}

	public async Task ProcessRegisterCheck(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		RezultCommand.Cash = DataCommand.Cash;
		RezultCommand.ElectronicPayment = DataCommand.ElectronicPayment;
		RezultCommand.AdvancePayment = DataCommand.AdvancePayment;
		RezultCommand.Credit = DataCommand.Credit;
		RezultCommand.CashProvision = DataCommand.CashProvision;
		DataCommand DataCommandPrintSlipEnd = null;
		RezultCommandProcessing RezultCommandProcessing = null;
		bool PrintSlipForEnd = false;
		string Slip = null;
		DataCommand DataCommandPrintSlip = null;
		if (DataCommand.RezultCommandProcessing != null)
		{
			Slip = DataCommand.RezultCommandProcessing.Slip;
		}
		if (DataCommand.IsFiscalCheck)
		{
			await CheckDataForFfd(DataCommand);
		}
		if (!(await CloseDocumentAndOpenShift(DataCommand, RezultCommand)))
		{
			return;
		}
		if (DataCommand.IsFiscalCheck)
		{
			await ComDevice.PostCheck(DataCommand, this);
			await MarkingCodeValidationFromCheck(DataCommand, RezultCommand, InCheck: true);
			if (Error != "")
			{
				return;
			}
		}
		string OldNetLog = "";
		bool isPayByProcessing;
		try
		{
			if (!DataCommand.PrintSlipAfterCheck.HasValue)
			{
				switch (SettDr.Paramets["PayByProcessing"])
				{
				case "1":
					DataCommand.PrintSlipAfterCheck = false;
					break;
				case "2":
					DataCommand.PrintSlipAfterCheck = true;
					break;
				case "3":
					DataCommand.PrintSlipAfterCheck = false;
					break;
				case "4":
					DataCommand.PrintSlipAfterCheck = true;
					break;
				}
			}
			if (!DataCommand.PrintSlipForCashier.HasValue)
			{
				switch (SettDr.Paramets["PayByProcessing"])
				{
				case "1":
					DataCommand.PrintSlipForCashier = false;
					break;
				case "2":
					DataCommand.PrintSlipForCashier = false;
					break;
				case "3":
					DataCommand.PrintSlipForCashier = true;
					break;
				case "4":
					DataCommand.PrintSlipForCashier = true;
					break;
				}
			}
			isPayByProcessing = NeedPayByProcessing(DataCommand, this);
			if (isPayByProcessing)
			{
				if (!DataCommand.NumDeviceByProcessing.HasValue)
				{
					DataCommand.NumDeviceByProcessing = int.Parse(SettDr.Paramets["NumDeviceByProcessing"]);
				}
				DataCommand dataCommand = new DataCommand();
				dataCommand.CommandMessage = "От чека: Команда на транзакцию";
				dataCommand.NumDevice = DataCommand.NumDeviceByProcessing.Value;
				if (DataCommand.TypeCheck == 0)
				{
					dataCommand.Command = "PayByPaymentCard";
				}
				else if (DataCommand.TypeCheck == 1 && string.IsNullOrEmpty(DataCommand.UniversalID))
				{
					dataCommand.Command = "ReturnPaymentByPaymentCard";
				}
				else if (DataCommand.TypeCheck == 1)
				{
					dataCommand.Command = "ReturnPaymentByPaymentCard";
				}
				dataCommand.Amount = DataCommand.ElectronicPayment;
				dataCommand.UniversalID = DataCommand.UniversalID;
				dataCommand.ReceiptNumber = DataCommand.ReceiptNumber;
				dataCommand.RRNCode = DataCommand.RRNCode;
				dataCommand.AuthorizationCode = DataCommand.AuthorizationCode;
				dataCommand.IdCommand = Guid.NewGuid().ToString();
				dataCommand.RunComPort = false;
				dataCommand.NotPrint = true;
				dataCommand.Timeout = 180;
				dataCommand.RunAsAddIn = DataCommand.RunAsAddIn;
				dataCommand.KeySubLicensing = DataCommand.KeySubLicensing;
				string textCommand = JsonConvert.SerializeObject(dataCommand, new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
				NetLogs.Append("\r\n\r\nВыполняем команду на эквайринг");
				OldNetLog = NetLogs.ToString();
				try
				{
					RezultCommandProcessing = (RezultCommandProcessing)Global.UnitManager.AddCommand_OLD(dataCommand, "", textCommand, "", DataCommand.IdCommand);
					RezultCommandProcessing = (RezultCommand.RezultProcessing = (RezultCommandProcessing)(await WaitWorkCommand(RezultCommandProcessing)));
					if (RezultCommandProcessing == null)
					{
						RezultCommandProcessing = new RezultCommandProcessing
						{
							Status = ExecuteStatus.Error,
							Error = "Не выполнена транзакция по оплате"
						};
						RezultCommand.RezultProcessing = RezultCommandProcessing;
					}
				}
				catch (Exception ex)
				{
					RezultCommandProcessing = new RezultCommandProcessing
					{
						Status = ExecuteStatus.Error,
						Error = "Не выполнена транзакция по оплате: " + ex.Message
					};
					RezultCommand.RezultProcessing = RezultCommandProcessing;
				}
				Slip = RezultCommandProcessing.Slip;
				NetLogs = new StringBuilder(OldNetLog);
				NetLogs.Append("\r\nСтатус выполнения эквайрига = " + RezultCommand.RezultProcessing.Status);
				if (RezultCommand.RezultProcessing.Status != ExecuteStatus.Ok)
				{
					RezultCommand.Status = ExecuteStatus.Error;
					Error = "Не выполнена транзакция по оплате: " + RezultCommand.RezultProcessing.Error;
					return;
				}
			}
		}
		catch (Exception ex2)
		{
			RezultCommand.Status = ExecuteStatus.Error;
			Error = ((Error == "") ? "" : (Error + ", ")) + Global.GetErrorMessagee(ex2);
			RezultCommand.Error = Error;
			NetLogs = new StringBuilder(OldNetLog);
			NetLogs.Append("\r\nСтатус выполнения эквайрига = 2");
			return;
		}
		try
		{
			if (Slip != null)
			{
				DataCommandPrintSlip = await CreateCommandSlip(Slip);
				DataCommandPrintSlip.CommandMessage = "От чека: Печать слипа";
				DataCommandPrintSlip.RunComPort = false;
				DataCommandPrintSlip.KeySubLicensing = DataCommand.KeySubLicensing;
			}
			if (RezultCommandProcessing != null && RezultCommandProcessing.Status != ExecuteStatus.Ok)
			{
				DataCommandPrintSlip.IsFiscalCheck = false;
				if (RezultCommandProcessing.Slip != null && RezultCommandProcessing.Slip != "")
				{
					NetLogs.Append("\r\n\r\nПечатаем слип ошибки эквайринга для клиента");
					await RegisterCheck(DataCommandPrintSlip, new RezultCommandKKm());
				}
				await PortCloseAsync();
				IsNotErrorStatus = false;
				RezultCommand.Status = ExecuteStatus.Error;
				Error = "Не выполнена транзакция по оплате: " + RezultCommandProcessing.Error;
				return;
			}
			if (Slip != null)
			{
				if (DataCommand.PrintSlipAfterCheck == false)
				{
					NetLogs.Append("\r\n\r\nДобавляем слип эквайринга в чек");
					if (DataCommandPrintSlip.CheckStrings.Length != 0)
					{
						DataCommand.CheckString[] array = new DataCommand.CheckString[DataCommand.CheckStrings.Length + DataCommandPrintSlip.CheckStrings.Length + 2];
						array[0] = new DataCommand.CheckString
						{
							PrintText = new DataCommand.PrintString
							{
								Text = ""
							}
						};
						int i;
						for (i = 0; i < DataCommandPrintSlip.CheckStrings.Length; i++)
						{
							array[1 + i] = DataCommandPrintSlip.CheckStrings[i];
						}
						array[i + 1] = new DataCommand.CheckString
						{
							PrintText = new DataCommand.PrintString
							{
								Text = ""
							}
						};
						for (int j = 0; j < DataCommand.CheckStrings.Length; j++)
						{
							array[2 + i + j] = DataCommand.CheckStrings[j];
						}
						DataCommand.CheckStrings = array;
					}
				}
				if (DataCommandPrintSlip.CheckStrings.Length != 0 && DataCommand.PrintSlipForCashier == true)
				{
					DataCommandPrintSlipEnd = DataCommandPrintSlip;
					PrintSlipForEnd = true;
				}
			}
		}
		catch (Exception)
		{
		}
		try
		{
			Error = "";
			if (isPayByProcessing)
			{
				NetLogs.Append("\r\n\r\nПечатаем чек для клиента");
			}
			RezultCommandCheck rezultCommandCheck = null;
			bool flag = ProcessRoute(DataCommand, RezultCommand, rezultCommandCheck);
			if (DataCommand.IsFiscalCheck && UseBuiltTerminal && UnitParamets["UseBuiltTerminal"].AsBool())
			{
				DataCommand.PayByProcessing = true;
			}
			else
			{
				DataCommand.PayByProcessing = false;
			}
			if (!DataCommand.IsFiscalCheck || !SettDr.Paramets["EmulationCheck"].AsBool())
			{
				await RegisterCheck(DataCommand, RezultCommand);
			}
			else
			{
				DataCommand noFiscalCheck = GetNoFiscalCheck(DataCommand, flag, RezultCommand);
				if (!flag)
				{
					await RegisterCheck(noFiscalCheck, RezultCommand);
				}
				else
				{
					await RegisterCheck(noFiscalCheck, new RezultCommandKKm());
				}
			}
			if (Error == "" && !DataCommand.IsGood)
			{
				Error = "Нет лицензии.";
			}
			if (Error != null && Error != "")
			{
				RezultCommand.Error = Error;
			}
			await PortCloseAsync();
		}
		catch (Exception ex4)
		{
			RezultCommand.Status = ExecuteStatus.Error;
			Error = Global.GetErrorMessagee(ex4) ?? "";
			RezultCommand.Error = Error;
			await PortCloseAsync();
		}
		if (RezultCommand.Status == ExecuteStatus.Ok && DataCommand.PrintSlipAfterCheck != false)
		{
			try
			{
				DataCommandPrintSlip.NotClearText = false;
				NetLogs.Append("\r\n\r\nПечатаем слип эквайринга для клиента");
				await RegisterCheck(DataCommandPrintSlip, new RezultCommandKKm());
				DataCommand.NotClearText = true;
			}
			catch
			{
			}
			await PortCloseAsync();
		}
		if (RezultCommand.Status == ExecuteStatus.Ok && DataCommandPrintSlipEnd != null && PrintSlipForEnd)
		{
			try
			{
				DataCommandPrintSlipEnd.NotClearText = false;
				NetLogs.Append("\r\n\r\nПечатаем слип чек для кассира");
				await RegisterCheck(DataCommandPrintSlipEnd, new RezultCommandKKm());
			}
			catch
			{
			}
			await PortCloseAsync();
		}
		if (RezultCommand.Status != ExecuteStatus.Ok && RezultCommand.RezultProcessing != null && RezultCommand.RezultProcessing.Status == ExecuteStatus.Ok)
		{
			NetLogs.Append("\r\n\r\nОтмена оплаты через эквайринг");
			DataCommand dataCommand2 = new DataCommand();
			dataCommand2.CommandMessage = "От чека: Отмена транзакции, Ошибка регистрации чека: " + RezultCommand.Error;
			dataCommand2.NumDevice = DataCommand.NumDeviceByProcessing.Value;
			dataCommand2.Command = "EmergencyReversal";
			dataCommand2.Amount = DataCommand.ElectronicPayment;
			dataCommand2.UniversalID = RezultCommand.RezultProcessing.UniversalID;
			dataCommand2.IdCommand = Guid.NewGuid().ToString();
			dataCommand2.RunComPort = false;
			dataCommand2.NotPrint = true;
			dataCommand2.KeySubLicensing = DataCommand.KeySubLicensing;
			string textCommand2 = JsonConvert.SerializeObject(dataCommand2, new JsonSerializerSettings
			{
				StringEscapeHandling = StringEscapeHandling.EscapeHtml
			});
			RezultCommandProcessing = (RezultCommandProcessing)Global.UnitManager.AddCommand_OLD(dataCommand2, "", textCommand2);
			RezultCommandProcessing = (RezultCommandProcessing)(await WaitWorkCommand(RezultCommandProcessing));
			RezultCommand.RezultProcessing.Slip = RezultCommand.RezultProcessing.Slip + "\r\n\r\n" + RezultCommandProcessing.Slip;
			DataCommandPrintSlip = await CreateCommandSlip(RezultCommandProcessing.Slip);
			DataCommandPrintSlip.RunComPort = false;
			DataCommandPrintSlip.KeySubLicensing = DataCommand.KeySubLicensing;
			if (DataCommandPrintSlip.CheckStrings.Length != 0)
			{
				DataCommandPrintSlip.NotClearText = true;
				await RegisterCheck(DataCommandPrintSlip, new RezultCommandKKm());
			}
		}
		Error = RezultCommand.Error;
		RezultCommand.Error = "";
	}

	public virtual async Task ValidationMarkingCode(DataCommand DataCommand, RezultMarkingCodeValidation RezultCommand, bool InCheck = false)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task ProcessValidationMarkingCode(DataCommand DataCommand, RezultMarkingCodeValidation RezultCommand, bool InCheck = false)
	{
		if (SettDr.Paramets["EmulationCheck"].AsBool())
		{
			RezultCommandCheck rezultCommandCheck = null;
			ProcessRoute(DataCommand, RezultCommand, rezultCommandCheck);
			return;
		}
		foreach (DataCommand.GoodCodeData GoodCodeData in DataCommand.GoodCodeDatas)
		{
			if (!GoodCodeData.MeasureOfQuantity.HasValue)
			{
				if (Math.Truncate(GoodCodeData.Quantity) == GoodCodeData.Quantity)
				{
					GoodCodeData.MeasureOfQuantity = 0u;
				}
				else
				{
					GoodCodeData.MeasureOfQuantity = 255u;
				}
			}
			if (GoodCodeData.MeasureOfQuantity == 0 && GoodCodeData.Quantity != 1m && !GoodCodeData.PackageQuantity.HasValue)
			{
				throw new Exception("При продаже штучного товара не из упаковки - количество должно быть равно строго 1 (шт)");
			}
			if (GoodCodeData.MeasureOfQuantity != 0 && GoodCodeData.PackageQuantity.HasValue)
			{
				throw new Exception("При продаже товара из упаковки - единица измерения должна быть строго 0 (шт. или ед.)");
			}
			if (GoodCodeData.MeasureOfQuantity != 0 && GoodCodeData.PackageQuantity.HasValue)
			{
				throw new Exception("При продаже товара из упаковки - мера количества предмета расчета (MeasureOfQuantity Тег ОФД 2108) должна быть СТРОГО равна 0 (шт)");
			}
			if (GoodCodeData.PackageQuantity.HasValue && GoodCodeData.PackageQuantity == 0)
			{
				throw new Exception("При продаже товара из упаковки - количество товара в упаковке (PackageQuantity Тег ОФД 1291) должна быть НЕ равна 0");
			}
			if (GoodCodeData.PackageQuantity.HasValue && GoodCodeData.Quantity != 1m)
			{
				throw new Exception("При продаже товара из упаковки - количество продаваемого товара должно быть строго 1 шт.");
			}
			if (GoodCodeData.PackageQuantity.HasValue)
			{
				decimal quantity = GoodCodeData.Quantity;
				decimal? num = GoodCodeData.PackageQuantity;
				if ((quantity > num.GetValueOrDefault()) & num.HasValue)
				{
					throw new Exception("При продаже товара из упаковки - количество продаваемого товара не может быть больше количества товара в упаковке (PackageQuantity Тег ОФД 1291)");
				}
			}
			if (GoodCodeData.BarCode == null && GoodCodeData.MarkingCodeBase64 != null)
			{
				byte[] array = Convert.FromBase64String(GoodCodeData.MarkingCodeBase64);
				GoodCodeData.BarCode = Encoding.UTF8.GetString(array);
				GoodCodeData.MarkingCodeBase64 = null;
			}
			if (GoodCodeData.TryBarCode == null || GoodCodeData.TryBarCode == "")
			{
				MarkingCode.DataProductCode dataProductCode = MarkingCode.ParseBarCode(GoodCodeData.BarCode);
				GoodCodeData.TryBarCode = dataProductCode.TryBarCode;
			}
			if (string.IsNullOrEmpty(GoodCodeData.TryBarCode))
			{
				continue;
			}
			RezultMarkingCodeValidation.tMarkingCodeValidation tMarkingCodeValidation = RezultCommand.MarkingCodeValidation.Find((RezultMarkingCodeValidation.tMarkingCodeValidation i) => i.BarCode == GoodCodeData.TryBarCode);
			if (tMarkingCodeValidation == null)
			{
				tMarkingCodeValidation = new RezultMarkingCodeValidation.tMarkingCodeValidation();
				tMarkingCodeValidation.Name = GoodCodeData.Name;
				tMarkingCodeValidation.BarCode = GoodCodeData.BarCode;
				tMarkingCodeValidation.TryBarCode = GoodCodeData.TryBarCode;
				if (GoodCodeData.IndustryProps != null && GoodCodeData.IndustryProps != "")
				{
					tMarkingCodeValidation.IndustryProps = GoodCodeData.IndustryProps;
					tMarkingCodeValidation.ValidationPR.ValidationResult = true;
					tMarkingCodeValidation.ValidationPR.ValidationDisabled = true;
					tMarkingCodeValidation.ValidationPR.DecryptionResult = "Проверка произведена вне KkmServer.";
				}
				RezultCommand.MarkingCodeValidation.Add(tMarkingCodeValidation);
			}
		}
		PortLogs.Append("Разрешительный режим: Старт проверки", "+");
		await PermitRegim.ValidationMarkingCode(this, DataCommand, RezultCommand, InCheck);
		PortLogs.Append("Разрешительный режим: Конец проверки проверки", "-");
		if (Kkm.FfdVersion >= 4)
		{
			PortLogs.Append("Проверка маркировки в ККТ: Старт проверки", "+");
			await ValidationMarkingCode(DataCommand, RezultCommand, InCheck);
			PortLogs.Append("Проверка маркировки в ККТ: Конец проверки проверки", "-");
		}
		if (string.IsNullOrEmpty(RezultCommand.Error))
		{
			foreach (RezultMarkingCodeValidation.tMarkingCodeValidation Item in RezultCommand.MarkingCodeValidation)
			{
				if (!Item.ValidationPR.ValidationResult && !Item.ValidationPR.ValidationDisabled && !DataCommand.GoodCodeDatas.Find((DataCommand.GoodCodeData i) => i.TryBarCode == Item.TryBarCode).AcceptOnBad)
				{
					if (Item.ValidationPR.DecryptionResult != null && Item.ValidationPR.DecryptionResult != "")
					{
						RezultCommand.Error = Item.ValidationPR.DecryptionResult;
					}
					else
					{
						RezultCommand.Error = "Проверка запретительного режима не пройдена";
					}
				}
			}
		}
		if (!string.IsNullOrEmpty(RezultCommand.Error))
		{
			Error = RezultCommand.Error;
		}
	}

	public async Task MarkingCodeValidationFromCheck(DataCommand DataCommand, RezultCommandKKm RezultCommand, bool InCheck = false)
	{
		DataCommand dataCommand = new DataCommand();
		RezultMarkingCodeValidation Rezult = new RezultMarkingCodeValidation();
		dataCommand.TypeCheck = DataCommand.TypeCheck;
		DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
		foreach (DataCommand.CheckString PrintString in checkStrings)
		{
			if (PrintString == null || PrintString.Register == null || PrintString.Register.GoodCodeData == null || string.IsNullOrEmpty(PrintString.Register.GoodCodeData.BarCode))
			{
				continue;
			}
			decimal num = Math.Round(PrintString.Register.Amount / PrintString.Register.Quantity, 2, MidpointRounding.ToEven);
			DataCommand.GoodCodeData goodCodeData = dataCommand.GoodCodeDatas.Find((DataCommand.GoodCodeData goodCodeData2) => goodCodeData2.BarCode == PrintString.Register.GoodCodeData.BarCode);
			if (goodCodeData == null)
			{
				goodCodeData = new DataCommand.GoodCodeData();
				goodCodeData.Name = PrintString.Register.Name;
				goodCodeData.BarCode = PrintString.Register.GoodCodeData.BarCode;
				goodCodeData.TryBarCode = PrintString.Register.GoodCodeData.TryBarCode;
				goodCodeData.AcceptOnBad = PrintString.Register.GoodCodeData.AcceptOnBad.Value;
				goodCodeData.MeasureOfQuantity = PrintString.Register.MeasureOfQuantity;
				goodCodeData.Quantity = PrintString.Register.Quantity;
				goodCodeData.Price = num;
				goodCodeData.PackageQuantity = PrintString.Register.PackageQuantity;
				goodCodeData.WaitForResult = true;
				goodCodeData.IndustryProps = PrintString.Register.GoodCodeData.IndustryProps;
				dataCommand.GoodCodeDatas.Add(goodCodeData);
			}
			else
			{
				goodCodeData.Quantity += PrintString.Register.Quantity;
				if (goodCodeData.Price > num && num != 0m)
				{
					goodCodeData.Price = num;
				}
			}
		}
		if (dataCommand.GoodCodeDatas.Count <= 0)
		{
			return;
		}
		try
		{
			await ProcessValidationMarkingCode(dataCommand, Rezult, InCheck);
		}
		catch (Exception ex)
		{
			throw new Exception(Global.GetErrorMessagee(ex), ex);
		}
		finally
		{
			RezultCommand.Error = Rezult.Error;
			List<RezultCommandKKm.tMarkingCodeValidation> list = new List<RezultCommandKKm.tMarkingCodeValidation>();
			foreach (RezultMarkingCodeValidation.tMarkingCodeValidation item in Rezult.MarkingCodeValidation)
			{
				RezultCommandKKm.tMarkingCodeValidation NewItem = new RezultCommandKKm.tMarkingCodeValidation();
				NewItem.Name = item.Name;
				NewItem.BarCode = item.BarCode;
				NewItem.IndustryProps = item.IndustryProps;
				NewItem.ValidationKKT.DecryptionResult = item.ValidationKKT.DecryptionResult;
				NewItem.ValidationKKT.ValidationResult = item.ValidationKKT.ValidationResult;
				NewItem.ValidationPR.DecryptionResult = item.ValidationPR.DecryptionResult;
				NewItem.ValidationPR.ValidationResult = item.ValidationPR.ValidationResult;
				NewItem.ValidationPR.ValidationDisabled = item.ValidationPR.ValidationDisabled;
				NewItem.ValidationPR.Log = item.ValidationPR.Result;
				NewItem.IndustryProps = item.IndustryProps;
				DataCommand.CheckString checkString = DataCommand.CheckStrings.ToList().Find((DataCommand.CheckString checkString3) => checkString3.Register != null && checkString3.Register.GoodCodeData != null && checkString3.Register.GoodCodeData.BarCode == NewItem.BarCode);
				if (checkString != null && !string.IsNullOrEmpty(checkString.Register.GoodCodeData.IndustryProps))
				{
					NewItem.ValidationPR.ValidationResult = false;
					NewItem.ValidationPR.ValidationDisabled = true;
					NewItem.IndustryProps = checkString.Register.GoodCodeData.IndustryProps;
					NewItem.ValidationPR.DecryptionResult = "Проверка произведена вне KkmServer";
					NewItem.ValidationPR.Log = null;
				}
				list.Add(NewItem);
				checkStrings = DataCommand.CheckStrings;
				foreach (DataCommand.CheckString checkString2 in checkStrings)
				{
					if (checkString2.Register != null && checkString2.Register.GoodCodeData != null && checkString2.Register.GoodCodeData.BarCode == item.BarCode)
					{
						checkString2.Register.GoodCodeData.IndustryProps = NewItem.IndustryProps;
						if (!string.IsNullOrEmpty(checkString2.Register.GoodCodeData.AddIndustryProps))
						{
							checkString2.Register.GoodCodeData.IndustryProps = NewItem.IndustryProps + checkString2.Register.GoodCodeData.AddIndustryProps;
						}
						checkString2.Register.GoodCodeData.Props1262 = IndustryProps1262;
						checkString2.Register.GoodCodeData.Props1263 = IndustryProps1263;
						checkString2.Register.GoodCodeData.Props1264 = IndustryProps1264;
						if (NewItem.ValidationPR.Log.HasValue && NewItem.ValidationPR.Log.Value.groupIds.Contains(7) && !string.IsNullOrEmpty(checkString2.Register.GoodCodeData.AddIndustryProps))
						{
							checkString2.Register.GoodCodeData.Props1262 = PharmaProps1262;
							checkString2.Register.GoodCodeData.Props1263 = PharmaProps1263;
							checkString2.Register.GoodCodeData.Props1264 = PharmaProps1264;
						}
					}
				}
			}
			RezultCommand.MarkingCodeValidation = list;
		}
	}

	public virtual async Task OpenShift(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task ProcessOpenShift(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		RezultCommandCheck rezultCommandCheck = null;
		bool flag = ProcessRoute(DataCommand, RezultCommand, rezultCommandCheck);
		if (!SettDr.Paramets["EmulationCheck"].AsBool())
		{
			await OpenShift(DataCommand, RezultCommand);
		}
		else
		{
			DataCommand noFiscalCheck = GetNoFiscalCheck(DataCommand, flag, RezultCommand);
			if (!flag)
			{
				await RegisterCheck(noFiscalCheck, RezultCommand);
			}
			else
			{
				await RegisterCheck(noFiscalCheck, new RezultCommandKKm());
			}
		}
		string OldError = Error;
		RezultCommandKKm RezultCommandTest = new RezultCommandKKm();
		DataCommand dataCommand = new DataCommand();
		dataCommand.Command = "GetDataKKT";
		dataCommand.NumDevice = DataCommand.NumDevice;
		dataCommand.KktNumber = DataCommand.KktNumber;
		dataCommand.InnKkm = DataCommand.InnKkm;
		dataCommand.TaxVariant = DataCommand.TaxVariant;
		dataCommand.IdCommand = Guid.NewGuid().ToString();
		dataCommand.RunComPort = true;
		NetLogs.Append("\r\n\r\nGetDataKKT");
		RezultCommandTest.RunComPort = true;
		await ProcessGetDataKKT(dataCommand, RezultCommandTest);
		await PortCloseAsync();
		RezultCommand.Info = RezultCommandTest.Info;
		Error = OldError;
	}

	public virtual async Task PaymentCashAndCloseShift(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		bool ClearText = true;
		bool CloseShiftWork = false;
		if (UnitParamets.ContainsKey("PaymentCashOnClouseShift") && UnitParamets["PaymentCashOnClouseShift"] == "True")
		{
			DataCommand dataCommand = new DataCommand();
			dataCommand.Command = "GetDataKKT";
			dataCommand.NumDevice = DataCommand.NumDevice;
			dataCommand.IdDevice = DataCommand.IdDevice;
			dataCommand.KktNumber = DataCommand.KktNumber;
			dataCommand.AdditionalActions = "fast";
			RezultCommandKKm rcRezultCommand = new RezultCommandKKm();
			await GetDataKKT(dataCommand, rcRezultCommand);
			if (rcRezultCommand.Info.SessionState == 3)
			{
				await ProcessCloseShift(DataCommand, RezultCommand, ClearText);
				CloseShiftWork = true;
			}
			if (rcRezultCommand.Info.BalanceCash > 0m)
			{
				DataCommand dcPaymentCash = new DataCommand
				{
					Command = "GetDataKKT",
					NumDevice = DataCommand.NumDevice,
					IdDevice = DataCommand.IdDevice,
					KktNumber = DataCommand.KktNumber,
					CashierName = DataCommand.CashierName,
					Amount = rcRezultCommand.Info.BalanceCash,
					AdditionalActions = "fast"
				};
				RezultCommandKKm RezPaymentCash = new RezultCommandKKm();
				await PaymentCash(dcPaymentCash, RezPaymentCash);
				if (RezPaymentCash.Status == ExecuteStatus.Error && !CloseShiftWork)
				{
					await ProcessCloseShift(DataCommand, RezultCommand, ClearText);
					CloseShiftWork = true;
					RezPaymentCash = new RezultCommandKKm();
					await PaymentCash(dcPaymentCash, RezPaymentCash);
				}
				ClearText = false;
			}
		}
		if (!CloseShiftWork)
		{
			await ProcessCloseShift(DataCommand, RezultCommand, ClearText);
		}
	}

	public virtual async Task CloseShift(DataCommand DataCommand, RezultCommandKKm RezultCommand, bool ClearText = false)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public async Task ProcessCloseShift(DataCommand DataCommand, RezultCommandKKm RezultCommand, bool ClearText = false)
	{
		if ((!UseBuiltTerminal || (UseBuiltTerminal && !UnitParamets["UseBuiltTerminal"].AsBool())) && !DataCommand.PayByProcessing.HasValue && SettDr.Paramets["PayByProcessing"] != "")
		{
			DataCommand.PayByProcessing = true;
		}
		if ((!UseBuiltTerminal || (UseBuiltTerminal && !UnitParamets["UseBuiltTerminal"].AsBool())) && DataCommand.PayByProcessing == true && SettDr.Paramets["SettlementInCloseShift"].AsBool())
		{
			try
			{
				DataCommand dataCommand = new DataCommand();
				dataCommand.NumDevice = int.Parse(SettDr.Paramets["NumDeviceByProcessing"]);
				dataCommand.Command = "Settlement";
				dataCommand.IdCommand = Guid.NewGuid().ToString();
				dataCommand.RunComPort = true;
				dataCommand.NotPrint = true;
				new Task(async delegate(object? Str)
				{
					RezultCommandProcessing rezultCommand = (RezultCommandProcessing)Global.UnitManager.AddCommand_OLD((DataCommand)Str, "", "");
					rezultCommand = (RezultCommandProcessing)(await WaitWorkCommand(rezultCommand));
					DataCommand dataCommand3 = await CreateCommandSlip(rezultCommand.Slip);
					if (dataCommand3.CheckStrings.Length != 0)
					{
						dataCommand3.NotClearText = true;
						await Global.UnitManager.AddCommand(dataCommand3, "", "");
					}
				}, dataCommand).Start();
			}
			catch
			{
			}
		}
		RezultCommandCheck rezultCommandCheck = null;
		bool flag = ProcessRoute(DataCommand, RezultCommand, rezultCommandCheck);
		if (!SettDr.Paramets["EmulationCheck"].AsBool())
		{
			await CloseShift(DataCommand, RezultCommand);
		}
		else
		{
			DataCommand noFiscalCheck = GetNoFiscalCheck(DataCommand, flag, RezultCommand);
			if (!flag)
			{
				await RegisterCheck(noFiscalCheck, RezultCommand);
			}
			else
			{
				await RegisterCheck(noFiscalCheck, new RezultCommandKKm());
			}
		}
		string OldError = Error;
		RezultCommandKKm RezultCommandTest = new RezultCommandKKm();
		DataCommand dataCommand2 = new DataCommand();
		dataCommand2.Command = "GetDataKKT";
		dataCommand2.NumDevice = DataCommand.NumDevice;
		dataCommand2.KktNumber = DataCommand.KktNumber;
		dataCommand2.InnKkm = DataCommand.InnKkm;
		dataCommand2.TaxVariant = DataCommand.TaxVariant;
		dataCommand2.IdCommand = Guid.NewGuid().ToString();
		dataCommand2.RunComPort = true;
		NetLogs.Append("\r\n\r\nGetDataKKT");
		RezultCommandTest.RunComPort = true;
		await ProcessGetDataKKT(dataCommand2, RezultCommandTest);
		await PortCloseAsync();
		RezultCommand.Info = RezultCommandTest.Info;
		Error = OldError;
	}

	public virtual async Task XReport(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "Не поддерживается текущим драйвером оборудования";
		RezultCommand.Error = Error;
		RezultCommand.Status = ExecuteStatus.Error;
	}

	public async Task ProcessXReport(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		if ((!UseBuiltTerminal || (UseBuiltTerminal && !UnitParamets["UseBuiltTerminal"].AsBool())) && !DataCommand.PayByProcessing.HasValue && SettDr.Paramets["PayByProcessing"] != "")
		{
			DataCommand.PayByProcessing = true;
		}
		if ((!UseBuiltTerminal || (UseBuiltTerminal && !UnitParamets["UseBuiltTerminal"].AsBool())) && DataCommand.PayByProcessing == true)
		{
			try
			{
				DataCommand dataCommand = new DataCommand();
				dataCommand.NumDevice = int.Parse(SettDr.Paramets["NumDeviceByProcessing"]);
				dataCommand.Command = "TerminalReport";
				dataCommand.Detailed = false;
				dataCommand.IdCommand = Guid.NewGuid().ToString();
				dataCommand.RunComPort = true;
				dataCommand.NotPrint = true;
				new Task(async delegate(object? Str)
				{
					RezultCommandProcessing rezultCommand = (RezultCommandProcessing)Global.UnitManager.AddCommand_OLD((DataCommand)Str, "", "");
					rezultCommand = (RezultCommandProcessing)(await WaitWorkCommand(rezultCommand));
					DataCommand dataCommand2 = await CreateCommandSlip(rezultCommand.Slip);
					if (dataCommand2.CheckStrings.Length != 0)
					{
						dataCommand2.NotClearText = true;
						await Global.UnitManager.AddCommand(dataCommand2, "", "");
					}
				}, dataCommand).Start();
			}
			catch
			{
			}
		}
		await XReport(DataCommand, RezultCommand);
	}

	public virtual async Task OfdReport(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		Error = "Не поддерживается текущим драйвером оборудования";
		RezultCommand.Error = Error;
		RezultCommand.Status = ExecuteStatus.Error;
	}

	public async Task ProcessOfdReport(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		await OfdReport(DataCommand, RezultCommand);
		RezultCommandKKm RezultCommandTest = new RezultCommandKKm();
		DataCommand dataCommand = new DataCommand();
		dataCommand.Command = "GetDataKKT";
		dataCommand.NumDevice = DataCommand.NumDevice;
		dataCommand.KktNumber = DataCommand.KktNumber;
		dataCommand.InnKkm = DataCommand.InnKkm;
		dataCommand.TaxVariant = DataCommand.TaxVariant;
		dataCommand.IdCommand = Guid.NewGuid().ToString();
		dataCommand.RunComPort = true;
		NetLogs.Append("\r\n\r\nGetDataKKT");
		RezultCommandTest.RunComPort = true;
		await ProcessGetDataKKT(dataCommand, RezultCommandTest);
		await PortCloseAsync();
		RezultCommand.Info = RezultCommandTest.Info;
	}

	public virtual async Task OpenCashDrawer(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task DepositingCash(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public async Task ProcessDepositingCash(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		RezultCommandCheck rezultCommandCheck = null;
		bool flag = ProcessRoute(DataCommand, RezultCommand, rezultCommandCheck);
		if (SettDr.Paramets["EmulationCheck"].AsBool())
		{
			DataCommand noFiscalCheck = GetNoFiscalCheck(DataCommand, flag, RezultCommand);
			if (flag)
			{
				await RegisterCheck(noFiscalCheck, new RezultCommandKKm());
			}
			else
			{
				await RegisterCheck(noFiscalCheck, RezultCommand);
			}
		}
		else
		{
			await DepositingCash(DataCommand, RezultCommand);
		}
	}

	public virtual async Task PaymentCash(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public async Task ProcessPaymentCash(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		RezultCommandCheck rezultCommandCheck = null;
		bool flag = ProcessRoute(DataCommand, RezultCommand, rezultCommandCheck);
		if (SettDr.Paramets["EmulationCheck"].AsBool())
		{
			DataCommand noFiscalCheck = GetNoFiscalCheck(DataCommand, flag, RezultCommand);
			if (flag)
			{
				await RegisterCheck(noFiscalCheck, new RezultCommandKKm());
			}
			else
			{
				await RegisterCheck(noFiscalCheck, RezultCommand);
			}
		}
		else
		{
			await PaymentCash(DataCommand, RezultCommand);
		}
	}

	public virtual async Task GetLineLength(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task KkmRegOfd(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public async Task ProcessKkmRegOfd(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		DataCommand.RegKkmOfd.RegNumber = DataCommand.RegKkmOfd.RegNumber.Trim().Replace(" ", "");
		DataCommand.RegKkmOfd.InnOfd = DataCommand.RegKkmOfd.InnOfd.Trim().Replace(" ", "");
		DataCommand.RegKkmOfd.InnOrganization = DataCommand.RegKkmOfd.InnOrganization.Trim().Replace(" ", "");
		DataCommand.RegKkmOfd.UrlOfd = DataCommand.RegKkmOfd.UrlOfd.Trim();
		DataCommand.RegKkmOfd.PortServerOfd = DataCommand.RegKkmOfd.PortServerOfd.Trim();
		DataCommand.RegKkmOfd.UrlServerOfd = DataCommand.RegKkmOfd.UrlServerOfd.Trim();
		DataCommand.RegKkmOfd.NameOrganization = DataCommand.RegKkmOfd.NameOrganization.Trim();
		DataCommand.RegKkmOfd.NameOFD = DataCommand.RegKkmOfd.NameOFD.Trim();
		DataCommand.RegKkmOfd.AddressSettle = DataCommand.RegKkmOfd.AddressSettle.Trim();
		DataCommand.RegKkmOfd.PlaceSettle = DataCommand.RegKkmOfd.PlaceSettle.Trim();
		DataCommand.RegKkmOfd.TaxVariant = DataCommand.RegKkmOfd.TaxVariant.Trim();
		if (DataCommand.RegKkmOfd.TaxVariant.Trim() == "" && (DataCommand.RegKkmOfd.Command == "Open" || DataCommand.RegKkmOfd.Command == "ChangeOrganization"))
		{
			Error = "Не указана система налогообложения!";
			RezultCommand.Status = ExecuteStatus.Error;
			return;
		}
		if (DataCommand.RegKkmOfd.SenderEmail == null)
		{
			DataCommand.RegKkmOfd.SenderEmail = "";
		}
		if (DataCommand.RegKkmOfd.Command != "Open")
		{
			DataCommand.RegKkmOfd.InnOrganization = Kkm.INN;
		}
		if (DataCommand.RegKkmOfd.Command != "Open" && DataCommand.RegKkmOfd.Command != "ChangeFN")
		{
			DataCommand.RegKkmOfd.KktNumber = Kkm.NumberKkm;
			DataCommand.RegKkmOfd.FnNumber = Kkm.Fn_Number;
			DataCommand.RegKkmOfd.RegNumber = Kkm.RegNumber;
		}
		if (DataCommand.RegKkmOfd.Command != "Open" && DataCommand.RegKkmOfd.Command != "ChangeOrganization")
		{
			DataCommand.RegKkmOfd.NameOrganization = Kkm.Organization;
			DataCommand.RegKkmOfd.AddressSettle = Kkm.AddressSettle;
			DataCommand.RegKkmOfd.PlaceSettle = Kkm.PlaceSettle;
			DataCommand.RegKkmOfd.SenderEmail = Kkm.SenderEmail;
			DataCommand.RegKkmOfd.TaxVariant = Kkm.TaxVariant;
			DataCommand.RegKkmOfd.SignOfAgent = Kkm.SignOfAgent;
		}
		if (DataCommand.RegKkmOfd.Command != "Open" && DataCommand.RegKkmOfd.Command != "ChangeOFD")
		{
			DataCommand.RegKkmOfd.UrlServerOfd = Kkm.UrlServerOfd;
			DataCommand.RegKkmOfd.PortServerOfd = Kkm.PortServerOfd;
			DataCommand.RegKkmOfd.NameOFD = Kkm.NameOFD;
			DataCommand.RegKkmOfd.UrlOfd = Kkm.UrlOfd;
			DataCommand.RegKkmOfd.InnOfd = Kkm.InnOfd;
		}
		if (DataCommand.RegKkmOfd.Command != "Open" && DataCommand.RegKkmOfd.Command != "ChangeKkm")
		{
			DataCommand.RegKkmOfd.EncryptionMode = Kkm.EncryptionMode;
			DataCommand.RegKkmOfd.OfflineMode = Kkm.OfflineMode;
			DataCommand.RegKkmOfd.InternetMode = Kkm.InternetMode;
			DataCommand.RegKkmOfd.BSOMode = Kkm.BSOMode;
			DataCommand.RegKkmOfd.AutomaticMode = Kkm.AutomaticMode;
			DataCommand.RegKkmOfd.AutomaticNumber = Kkm.AutomaticNumber;
			DataCommand.RegKkmOfd.PrinterAutomatic = Kkm.PrinterAutomatic;
			DataCommand.RegKkmOfd.ServiceMode = Kkm.ServiceMode;
			DataCommand.RegKkmOfd.SignOfGambling = Kkm.SignOfGambling;
			DataCommand.RegKkmOfd.SignOfLottery = Kkm.SignOfLottery;
			DataCommand.RegKkmOfd.SaleExcisableGoods = Kkm.SaleExcisableGoods;
		}
		if (string.IsNullOrEmpty(DataCommand.CashierName))
		{
			Error = "Не указано ФИО кассира!";
			RezultCommand.Status = ExecuteStatus.Error;
		}
		else
		{
			await KkmRegOfd(DataCommand, RezultCommand);
		}
	}

	public virtual async Task GetDataKKT(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		UpdateSettingsServer();
		RezultCommand.Info = new DataCommand.TypeRegKkmOfd();
		RezultCommand.Info.RegNumber = Kkm.RegNumber;
		RezultCommand.Info.KktNumber = SettDr.NumberKkm;
		RezultCommand.Info.FN_IsFiscal = Kkm.FN_IsFiscal;
		RezultCommand.Info.FN_MemOverflowl = Kkm.FN_MemOverflowl;
		RezultCommand.Info.FnNumber = Kkm.Fn_Number;
		RezultCommand.Info.FN_DateStart = Kkm.FN_DateStart;
		RezultCommand.Info.FN_DateEnd = Kkm.FN_DateEnd;
		RezultCommand.Info.NameOrganization = Kkm.Organization;
		RezultCommand.Info.InnOrganization = SettDr.INN;
		RezultCommand.Info.AddressSettle = Kkm.AddressSettle;
		RezultCommand.Info.PlaceSettle = Kkm.PlaceSettle;
		RezultCommand.Info.TaxVariant = SettDr.TaxVariant;
		RezultCommand.Info.SignOfAgent = Kkm.SignOfAgent;
		RezultCommand.Info.OfflineMode = Kkm.OfflineMode;
		RezultCommand.Info.ServiceMode = Kkm.ServiceMode;
		RezultCommand.Info.BSOMode = Kkm.BSOMode;
		RezultCommand.Info.EncryptionMode = Kkm.EncryptionMode;
		RezultCommand.Info.InternetMode = Kkm.InternetMode;
		RezultCommand.Info.AutomaticMode = Kkm.AutomaticMode;
		RezultCommand.Info.SignOfGambling = Kkm.SignOfGambling;
		RezultCommand.Info.SignOfLottery = Kkm.SignOfLottery;
		RezultCommand.Info.SaleMarking = Kkm.SaleMarking;
		RezultCommand.Info.SignPawnshop = Kkm.SignPawnshop;
		RezultCommand.Info.SignAssurance = Kkm.SignAssurance;
		RezultCommand.Info.NameOFD = Kkm.NameOFD;
		RezultCommand.Info.InnOfd = Kkm.InnOfd;
		RezultCommand.Info.UrlServerOfd = Kkm.UrlServerOfd;
		RezultCommand.Info.PortServerOfd = Kkm.PortServerOfd;
		RezultCommand.Info.PaperOver = Kkm.PaperOver;
		RezultCommand.Info.UrlOfd = Kkm.UrlOfd;
		RezultCommand.Info.SenderEmail = Kkm.SenderEmail;
		RezultCommand.Info.SaleExcisableGoods = Kkm.SaleExcisableGoods;
		RezultCommand.Info.AutomaticNumber = Kkm.AutomaticNumber;
		RezultCommand.Info.PrinterAutomatic = Kkm.PrinterAutomatic;
		RezultCommand.Info.OFD_Error = Kkm.OFD_Error;
		RezultCommand.Info.OFD_NumErrorDoc = Kkm.OFD_NumErrorDoc;
		RezultCommand.Info.OFD_DateErrorDoc = Kkm.OFD_DateErrorDoc;
		RezultCommand.Info.SessionState = SessionOpen;
		RezultCommand.Info.OnOff = SettDr.Active;
		RezultCommand.Info.Active = IsInit;
		if (Kkm.FfdVersion == 1)
		{
			RezultCommand.Info.FFDVersion = "1.0";
			RezultCommand.Info.FFDVersionFN = "1.0";
			RezultCommand.Info.FFDVersionKKT = "1.0";
		}
		else if (Kkm.FfdVersion == 2)
		{
			RezultCommand.Info.FFDVersion = "1.05";
			RezultCommand.Info.FFDVersionFN = "1.0";
			RezultCommand.Info.FFDVersionKKT = "1.1";
		}
		else if (Kkm.FfdVersion == 3)
		{
			RezultCommand.Info.FFDVersion = "1.1";
			RezultCommand.Info.FFDVersionFN = "1.1";
			RezultCommand.Info.FFDVersionKKT = "1.1";
		}
		else if (Kkm.FfdVersion == 4)
		{
			RezultCommand.Info.FFDVersion = "1.2";
			RezultCommand.Info.FFDVersionFN = "1.2";
			RezultCommand.Info.FFDVersionKKT = "1.2";
		}
		RezultCommand.Info.DateTimeKKT = Kkm.DateTimeKKT;
		RezultCommand.Info.Firmware_Version = Kkm.Firmware_Version;
		RezultCommand.Info.Firmware_Status = Kkm.Firmware_Status;
		ComDevice.InDate inDate = await ComDevice.ReadComDevice(this, AllowDateAction: false, AllowCount: true, OnlySerial: false, PluzBesplatnoe: true);
		RezultCommand.Info.LicenseExpirationDate = ((inDate.DateTime != default(DateTime)) ? inDate.DateTime.AddDays(1.0) : inDate.DateTime.AddDays(1.0));
		RezultCommand.LineLength = Kkm.PrintingWidth;
		RezultCommand.Status = ExecuteStatus.Ok;
		SaveParemeterSearhKKT();
	}

	public async Task ProcessGetDataKKT(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор)
		{
			try
			{
				await ComDevice.PostCheck(DataCommand, this, NewTask: true, NoRandom: true, NotPrint: true);
			}
			catch
			{
			}
		}
		RezultCommandCheck rezultCommandCheck = null;
		if (ProcessRoute(DataCommand, RezultCommand, rezultCommandCheck))
		{
			if (RezultCommand.Status == ExecuteStatus.Ok && RezultCommand.Info != null)
			{
				Kkm.RegNumber = RezultCommand.Info.RegNumber;
				Kkm.NumberKkm = RezultCommand.Info.KktNumber;
				Kkm.FN_IsFiscal = RezultCommand.Info.FN_IsFiscal;
				Kkm.FN_MemOverflowl = RezultCommand.Info.FN_MemOverflowl;
				Kkm.Fn_Number = RezultCommand.Info.FnNumber;
				Kkm.FN_DateStart = RezultCommand.Info.FN_DateStart;
				Kkm.FN_DateEnd = RezultCommand.Info.FN_DateEnd;
				Kkm.Organization = RezultCommand.Info.NameOrganization;
				Kkm.INN = RezultCommand.Info.InnOrganization;
				Kkm.AddressSettle = RezultCommand.Info.AddressSettle;
				Kkm.PlaceSettle = RezultCommand.Info.PlaceSettle;
				Kkm.TaxVariant = RezultCommand.Info.TaxVariant;
				Kkm.SignOfAgent = RezultCommand.Info.SignOfAgent;
				Kkm.OfflineMode = RezultCommand.Info.OfflineMode;
				Kkm.ServiceMode = RezultCommand.Info.ServiceMode;
				Kkm.BSOMode = RezultCommand.Info.BSOMode;
				Kkm.EncryptionMode = RezultCommand.Info.EncryptionMode;
				Kkm.InternetMode = RezultCommand.Info.InternetMode;
				Kkm.AutomaticMode = RezultCommand.Info.AutomaticMode;
				Kkm.SignOfGambling = RezultCommand.Info.SignOfGambling;
				Kkm.SignOfLottery = RezultCommand.Info.SignOfLottery;
				Kkm.SaleMarking = RezultCommand.Info.SaleMarking;
				Kkm.SignPawnshop = RezultCommand.Info.SignPawnshop;
				Kkm.SignAssurance = RezultCommand.Info.SignAssurance;
				Kkm.NameOFD = RezultCommand.Info.NameOFD;
				Kkm.InnOfd = RezultCommand.Info.InnOfd;
				Kkm.UrlServerOfd = RezultCommand.Info.UrlServerOfd;
				Kkm.PortServerOfd = RezultCommand.Info.PortServerOfd;
				Kkm.PaperOver = RezultCommand.Info.PaperOver;
				Kkm.UrlOfd = RezultCommand.Info.UrlOfd;
				Kkm.SenderEmail = RezultCommand.Info.SenderEmail;
				Kkm.SaleExcisableGoods = RezultCommand.Info.SaleExcisableGoods;
				Kkm.AutomaticNumber = RezultCommand.Info.AutomaticNumber;
				Kkm.PrinterAutomatic = RezultCommand.Info.PrinterAutomatic;
				Kkm.OFD_Error = RezultCommand.Info.OFD_Error;
				Kkm.OFD_NumErrorDoc = RezultCommand.Info.OFD_NumErrorDoc;
				Kkm.OFD_DateErrorDoc = RezultCommand.Info.OFD_DateErrorDoc;
				SessionOpen = RezultCommand.Info.SessionState;
				if (RezultCommand.Info.FFDVersion == "1.0")
				{
					Kkm.FfdVersion = 1;
				}
				else if (RezultCommand.Info.FFDVersion == "1.05")
				{
					Kkm.FfdVersion = 2;
				}
				else if (RezultCommand.Info.FFDVersion == "1.1")
				{
					Kkm.FfdVersion = 3;
				}
				else if (RezultCommand.Info.FFDVersion == "1.2")
				{
					Kkm.FfdVersion = 4;
				}
				Kkm.DateTimeKKT = RezultCommand.Info.DateTimeKKT;
				Kkm.Firmware_Version = RezultCommand.Info.Firmware_Version;
				Kkm.Firmware_Status = RezultCommand.Info.Firmware_Status;
			}
		}
		else if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор)
		{
			await GetDataKKT(DataCommand, RezultCommand);
		}
		else if (SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
		{
			RezultCommand.Info = new DataCommand.TypeRegKkmOfd();
			RezultCommand.Info.RegNumber = "000000000";
			RezultCommand.Info.KktNumber = "00000000000";
			RezultCommand.Info.FN_IsFiscal = SettDr.Paramets["EmulationCheck"].AsBool();
			RezultCommand.Info.FN_MemOverflowl = false;
			RezultCommand.Info.FnNumber = "00000000000";
			RezultCommand.Info.FN_DateStart = new DateTime(2000, 1, 1);
			RezultCommand.Info.FN_DateEnd = new DateTime(2099, 1, 1);
			RezultCommand.Info.NameOrganization = SettDr.Paramets["INN"];
			RezultCommand.Info.InnOrganization = SettDr.Paramets["INN"];
			RezultCommand.Info.TaxVariant = "0,1,2,3,4,5";
			RezultCommand.Info.FFDVersion = "4";
			RezultCommand.Info.SessionState = 2;
		}
	}

	public virtual async Task<bool> CloseDocumentAndOpenShift(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		return true;
	}

	public virtual async Task ReadStatusOFD(bool Full = false, bool ReadInfoGer = false, bool NoInit = false)
	{
	}

	public virtual async Task GetCheckAndSession(RezultCommandKKm RezultCommand, bool IsSessionNumber = true, bool IsCheckNumber = true)
	{
	}

	public virtual async Task CheckDataForFfd(DataCommand DataCommand)
	{
		if (DataCommand.IsFiscalCheck)
		{
			if (DataCommand.ClientAddress == null)
			{
				DataCommand.ClientAddress = "";
			}
			if (DataCommand.SenderEmail == null)
			{
				DataCommand.SenderEmail = "";
			}
			if (DataCommand.TaxVariant == null)
			{
				DataCommand.TaxVariant = "";
			}
			if (!Kkm.InternetMode && !Kkm.AutomaticMode && (DataCommand.CashierName == null || DataCommand.CashierName == ""))
			{
				throw new SystemException("Не указан кассир");
			}
			if (DataCommand.CashierVATIN != null && DataCommand.CashierVATIN.Trim() != "" && DataCommand.CashierVATIN.Trim().Length != 12)
			{
				throw new SystemException("ИНН кассира должен содержать 12 символов");
			}
			if (DataCommand.CashLessType1 > 0m)
			{
				throw new SystemException("Нельзя указывать поле \"CashLessType1\" для ККТ работающее по ФФД >= 1.05");
			}
			if (DataCommand.CashLessType2 > 0m)
			{
				throw new SystemException("Нельзя указывать поле \"CashLessType2\" для ККТ работающее по ФФД >= 1.05");
			}
			if (DataCommand.CashLessType3 > 0m)
			{
				throw new SystemException("Нельзя указывать поле \"CashLessType3\" для ККТ работающее по ФФД >= 1.05");
			}
			if (DataCommand.CheckProps != null && DataCommand.CheckProps.Length != 0)
			{
				throw new SystemException("Нельзя указывать поле \"CheckProps\" для ККТ работающее по ФФД >= 1.05");
			}
			if (!string.IsNullOrEmpty(DataCommand.ClientINN) && !ChekINN(DataCommand.ClientINN))
			{
				throw new SystemException("Некорректный ИНН клиента");
			}
			if (!string.IsNullOrEmpty(DataCommand.CashierVATIN) && !ChekINN(DataCommand.CashierVATIN))
			{
				throw new SystemException("Некорректный ИНН кассира");
			}
			if (DataCommand.CheckStrings != null)
			{
				List<DataCommand.CheckString> list = new List<DataCommand.CheckString>();
				DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
				foreach (DataCommand.CheckString checkString in checkStrings)
				{
					if (checkString != null)
					{
						list.Add(checkString);
					}
				}
				DataCommand.CheckStrings = list.ToArray();
			}
			int num = 0;
			if (DataCommand.CheckStrings != null)
			{
				DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
				foreach (DataCommand.CheckString checkString2 in checkStrings)
				{
					if (DataCommand.IsFiscalCheck && checkString2 != null && checkString2.Register != null)
					{
						num++;
					}
					if (DataCommand.IsFiscalCheck && checkString2 != null && checkString2.Register != null && checkString2.Register.Quantity != 0m)
					{
						if (checkString2.Register.Name == null)
						{
							throw new SystemException("Не указано наименование товара/услуги!");
						}
						if (checkString2.Register.Name.Length > 128)
						{
							checkString2.Register.Name = checkString2.Register.Name.Substring(0, 126) + "..";
						}
						if (checkString2.Register.Quantity == 0m)
						{
							throw new SystemException("Не указано количество товара/услуги!");
						}
						if (!checkString2.Register.SignMethodCalculation.HasValue)
						{
							throw new SystemException("Не указан признак способа расчета!");
						}
						if (!checkString2.Register.SignCalculationObject.HasValue)
						{
							throw new SystemException("Не указан признак предмета расчета!");
						}
						_ = checkString2.Register.Tax;
						if (checkString2.Register.Tax != -1m && checkString2.Register.Tax != 0m && checkString2.Register.Tax != 5m && checkString2.Register.Tax != 7m && checkString2.Register.Tax != 10m && checkString2.Register.Tax != 20m && checkString2.Register.Tax != 22m && checkString2.Register.Tax != 105m && checkString2.Register.Tax != 107m && checkString2.Register.Tax != 120m && checkString2.Register.Tax != 122m && checkString2.Register.Tax != 110m)
						{
							throw new SystemException("Не правильно указана ставка НДС!");
						}
					}
				}
			}
			if (DataCommand.CheckStrings != null)
			{
				DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
				foreach (DataCommand.CheckString checkString3 in checkStrings)
				{
					if (checkString3.PrintText != null && (checkString3.PrintText.Font < 0 || checkString3.PrintText.Font > 4))
					{
						throw new SystemException($"Номер шрифта: '{checkString3.PrintText.Font}' - не корректен!");
					}
				}
			}
			if (DataCommand.TypeCheck == 2 || DataCommand.TypeCheck == 3 || DataCommand.TypeCheck == 12 || DataCommand.TypeCheck == 13)
			{
				if (DataCommand.CorrectionBaseNumber == null || DataCommand.CorrectionBaseNumber == "")
				{
					throw new SystemException("Не указан номер корректировки");
				}
				if (!DataCommand.CorrectionBaseDate.HasValue || DataCommand.CorrectionBaseDate == default(DateTime))
				{
					throw new SystemException("Не указана дата корректировки");
				}
				if (num == 0 && (DataCommand.Amount != 0m || DataCommand.SumTaxNone != 0m || DataCommand.SumTax22 != 0m || DataCommand.SumTax20 != 0m || DataCommand.SumTax10 != 0m || DataCommand.SumTax0 != 0m || DataCommand.SumTax122 != 0m || DataCommand.SumTax120 != 0m || DataCommand.SumTax110 != 0m))
				{
					DataCommand.Register register = new DataCommand.Register();
					register.Name = "Чек корректировки";
					register.Quantity = 1m;
					register.Price = DataCommand.Amount;
					register.Amount = DataCommand.Amount;
					if (DataCommand.SumTaxNone != 0m)
					{
						register.Tax = -1m;
					}
					else if (DataCommand.SumTax22 != 0m)
					{
						register.Tax = 22m;
					}
					else if (DataCommand.SumTax20 != 0m)
					{
						register.Tax = 20m;
					}
					else if (DataCommand.SumTax10 != 0m)
					{
						register.Tax = 10m;
					}
					else if (DataCommand.SumTax5 != 0m)
					{
						register.Tax = 5m;
					}
					else if (DataCommand.SumTax7 != 0m)
					{
						register.Tax = 7m;
					}
					else if (DataCommand.SumTax0 != 0m)
					{
						register.Tax = default(decimal);
					}
					else if (DataCommand.SumTax122 != 0m)
					{
						register.Tax = 122m;
					}
					else if (DataCommand.SumTax120 != 0m)
					{
						register.Tax = 120m;
					}
					else if (DataCommand.SumTax110 != 0m)
					{
						register.Tax = 110m;
					}
					else if (DataCommand.SumTax105 != 0m)
					{
						register.Tax = 105m;
					}
					else if (DataCommand.SumTax107 != 0m)
					{
						register.Tax = 107m;
					}
					register.SignMethodCalculation = 4;
					register.SignCalculationObject = 1;
					DataCommand.CheckString checkString4 = new DataCommand.CheckString();
					checkString4.Register = register;
					Array.Resize(ref DataCommand.CheckStrings, DataCommand.CheckStrings.Length + 1);
					if (DataCommand.CheckStrings == null)
					{
						DataCommand.CheckStrings = new DataCommand.CheckString[0];
					}
					DataCommand.CheckStrings[DataCommand.CheckStrings.Length - 1] = checkString4;
					num = 1;
				}
				DataCommand.Amount = default(decimal);
				DataCommand.SumTaxNone = default(decimal);
				DataCommand.SumTax22 = default(decimal);
				DataCommand.SumTax20 = default(decimal);
				DataCommand.SumTax10 = default(decimal);
				DataCommand.SumTax5 = default(decimal);
				DataCommand.SumTax7 = default(decimal);
				DataCommand.SumTax0 = default(decimal);
				DataCommand.SumTax122 = default(decimal);
				DataCommand.SumTax120 = default(decimal);
				DataCommand.SumTax110 = default(decimal);
				DataCommand.SumTax105 = default(decimal);
				DataCommand.SumTax107 = default(decimal);
				if (DataCommand.CheckStrings != null)
				{
					DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
					foreach (DataCommand.CheckString checkString5 in checkStrings)
					{
						if (checkString5 != null && checkString5.Register != null && checkString5.Register.Quantity != 0m)
						{
							DataCommand.Amount += checkString5.Register.Amount;
							if (checkString5.Register.Tax == -1m)
							{
								DataCommand.SumTaxNone += checkString5.Register.Amount;
							}
							else if (checkString5.Register.Tax == 22m)
							{
								DataCommand.SumTax22 += Math.Round(checkString5.Register.Amount / 122m * 22m, 2, MidpointRounding.AwayFromZero);
							}
							else if (checkString5.Register.Tax == 20m)
							{
								DataCommand.SumTax20 += Math.Round(checkString5.Register.Amount / 120m * 20m, 2, MidpointRounding.AwayFromZero);
							}
							else if (checkString5.Register.Tax == 10m)
							{
								DataCommand.SumTax10 += Math.Round(checkString5.Register.Amount / 110m * 10m, 2, MidpointRounding.AwayFromZero);
							}
							else if (checkString5.Register.Tax == 5m)
							{
								DataCommand.SumTax5 += Math.Round(checkString5.Register.Amount / 105m * 5m, 2, MidpointRounding.AwayFromZero);
							}
							else if (checkString5.Register.Tax == 7m)
							{
								DataCommand.SumTax10 += Math.Round(checkString5.Register.Amount / 107m * 7m, 2, MidpointRounding.AwayFromZero);
							}
							else if (checkString5.Register.Tax == 0m)
							{
								DataCommand.SumTax0 += checkString5.Register.Amount;
							}
							else if (checkString5.Register.Tax == 122m)
							{
								DataCommand.SumTax122 += Math.Round(checkString5.Register.Amount / 100m * 22m, 2, MidpointRounding.AwayFromZero);
							}
							else if (checkString5.Register.Tax == 120m)
							{
								DataCommand.SumTax120 += Math.Round(checkString5.Register.Amount / 100m * 20m, 2, MidpointRounding.AwayFromZero);
							}
							else if (checkString5.Register.Tax == 110m)
							{
								DataCommand.SumTax110 += Math.Round(checkString5.Register.Amount / 100m * 10m, 2, MidpointRounding.AwayFromZero);
							}
							else if (checkString5.Register.Tax == 105m)
							{
								DataCommand.SumTax105 += Math.Round(checkString5.Register.Amount / 100m * 5m, 2, MidpointRounding.AwayFromZero);
							}
							else if (checkString5.Register.Tax == 107m)
							{
								DataCommand.SumTax107 += Math.Round(checkString5.Register.Amount / 100m * 7m, 2, MidpointRounding.AwayFromZero);
							}
						}
					}
				}
			}
			if (num == 0)
			{
				throw new SystemException("В чеке нет фискальных строк!!!");
			}
			if (DataCommand.CheckStrings != null)
			{
				DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
				foreach (DataCommand.CheckString checkString6 in checkStrings)
				{
					if (!DataCommand.IsFiscalCheck || checkString6 == null || checkString6.Register == null || !(checkString6.Register.Quantity != 0m))
					{
						continue;
					}
					if (checkString6.Register.GoodCodeData != null && !checkString6.Register.GoodCodeData.AcceptOnBad.HasValue)
					{
						checkString6.Register.GoodCodeData.AcceptOnBad = Global.Settings.MarkingCodeAcceptOnBad;
					}
					if (!checkString6.Register.MeasureOfQuantity.HasValue)
					{
						if (Math.Truncate(checkString6.Register.Quantity) == checkString6.Register.Quantity)
						{
							checkString6.Register.MeasureOfQuantity = 0u;
						}
						else
						{
							checkString6.Register.MeasureOfQuantity = 255u;
						}
					}
					if (checkString6.Register.GoodCodeData != null && (checkString6.Register.GoodCodeData.BarCode == null || checkString6.Register.GoodCodeData.BarCode == "") && (checkString6.Register.GoodCodeData.MarkingCodeBase64 == null || checkString6.Register.GoodCodeData.MarkingCodeBase64 == "") && (checkString6.Register.GoodCodeData.SerialNumber == null || checkString6.Register.GoodCodeData.SerialNumber == ""))
					{
						checkString6.Register.GoodCodeData = null;
					}
					if (Kkm.FfdVersion >= 4 && checkString6.Register.GoodCodeData != null)
					{
						if (checkString6.Register.GoodCodeData.BarCode != null)
						{
							MarkingCode.DataProductCode dataProductCode = MarkingCode.ParseBarCode(checkString6.Register.GoodCodeData.BarCode);
							checkString6.Register.GoodCodeData.TryBarCode = dataProductCode.TryBarCode;
						}
						else if (checkString6.Register.GoodCodeData.MarkingCodeBase64 != null)
						{
							byte[] array = Convert.FromBase64String(checkString6.Register.GoodCodeData.MarkingCodeBase64);
							checkString6.Register.GoodCodeData.BarCode = Encoding.UTF8.GetString(array);
							checkString6.Register.GoodCodeData.MarkingCodeBase64 = null;
							MarkingCode.DataProductCode dataProductCode2 = MarkingCode.ParseBarCode(checkString6.Register.GoodCodeData.BarCode);
							checkString6.Register.GoodCodeData.TryBarCode = dataProductCode2.TryBarCode;
						}
						if (checkString6.Register.MeasureOfQuantity == 0 && checkString6.Register.Quantity != 1m && !checkString6.Register.PackageQuantity.HasValue)
						{
							throw new Exception("При продаже штучного товара не из упаковки - количество должно быть равно строго 1 (шт)");
						}
						if (checkString6.Register.MeasureOfQuantity != 0 && checkString6.Register.PackageQuantity.HasValue)
						{
							throw new Exception("При продаже товара из упаковки - единица измерения должна быть строго 0 (шт. или ед.)");
						}
						if (checkString6.Register.MeasureOfQuantity != 0 && checkString6.Register.PackageQuantity.HasValue)
						{
							throw new Exception("При продаже товара из упаковки - мера количества предмета расчета (MeasureOfQuantity Тег ОФД 2108) должна быть СТРОГО равна 0 (шт)");
						}
						if (checkString6.Register.PackageQuantity.HasValue && checkString6.Register.PackageQuantity == 0)
						{
							throw new Exception("При продаже товара из упаковки - количество товара в упаковке (PackageQuantity Тег ОФД 1291) должна быть НЕ равна 0");
						}
						if (checkString6.Register.PackageQuantity.HasValue && checkString6.Register.Quantity != 1m)
						{
							throw new Exception("При продаже товара из упаковки - количество продаваемого товара должно быть строго 1 шт.");
						}
						if (checkString6.Register.PackageQuantity.HasValue)
						{
							decimal quantity = checkString6.Register.Quantity;
							decimal? num2 = checkString6.Register.PackageQuantity;
							if ((quantity > num2.GetValueOrDefault()) & num2.HasValue)
							{
								throw new Exception("При продаже товара из упаковки - количество продаваемого товара не может быть больше количества товара в упаковке (PackageQuantity Тег ОФД 1291)");
							}
						}
						if (checkString6.Register.GoodCodeData.BarCode != null && checkString6.Register.SignCalculationObject == 1 && UnitParamets["KktChangeSCO_31_32"].ToLower() == "true".ToString().ToLower())
						{
							checkString6.Register.SignCalculationObject = 33;
						}
						if (checkString6.Register.GoodCodeData.BarCode != null && checkString6.Register.SignCalculationObject == 2 && UnitParamets["KktChangeSCO_31_32"].ToLower() == "true".ToString().ToLower())
						{
							checkString6.Register.SignCalculationObject = 31;
						}
					}
					else if (Kkm.FfdVersion <= 3 && checkString6.Register.GoodCodeData != null)
					{
						if (string.IsNullOrEmpty(checkString6.Register.GoodCodeData.MarkingCodeBase64))
						{
							if (!string.IsNullOrEmpty(checkString6.Register.GoodCodeData.BarCode))
							{
								MarkingCode.DataProductCode dataProductCode3 = MarkingCode.ParseBarCode(checkString6.Register.GoodCodeData.BarCode);
								if (dataProductCode3.isParsed && !checkString6.Register.GoodCodeData.ContainsSerialNumber)
								{
									checkString6.Register.GoodCodeData.MarkingCodeBase64 = dataProductCode3.MarkingCodeBase64;
								}
								else if (dataProductCode3.isParsed && checkString6.Register.GoodCodeData.ContainsSerialNumber && dataProductCode3.ContainsSerialNumber)
								{
									checkString6.Register.GoodCodeData.MarkingCodeBase64 = dataProductCode3.MarkingCodeBase64;
								}
								else
								{
									if (dataProductCode3.isParsed && checkString6.Register.GoodCodeData.ContainsSerialNumber && !dataProductCode3.ContainsSerialNumber)
									{
										throw new SystemException($"'Код Маркровки' '{checkString6.Register.GoodCodeData.BarCode}' не является ШК с идентификатором экземпляра товара");
									}
									if (!dataProductCode3.isParsed)
									{
										throw new SystemException($"Ошибка разбора 'Кода Маркровки' '{checkString6.Register.GoodCodeData.BarCode}': '{dataProductCode3.Errors}'");
									}
								}
							}
							else if (!string.IsNullOrEmpty(checkString6.Register.GoodCodeData.GTIN))
							{
								byte[] data = GetData1162(checkString6.Register.GoodCodeData);
								checkString6.Register.GoodCodeData.MarkingCodeBase64 = Convert.ToBase64String(data);
							}
						}
						if (checkString6.Register.GoodCodeData.MarkingCodeBase64 == "AAA=")
						{
							checkString6.Register.GoodCodeData = null;
						}
					}
					if (checkString6.Register.SignCalculationObject == 33 && UnitParamets["KktChangeSCO_31_32"].ToLower() == "back".ToString().ToLower())
					{
						checkString6.Register.SignCalculationObject = 1;
					}
					if (checkString6.Register.SignCalculationObject == 31 && UnitParamets["KktChangeSCO_31_32"].ToLower() == "back".ToString().ToLower())
					{
						checkString6.Register.SignCalculationObject = 2;
					}
				}
			}
		}
		if (DataCommand.IsFiscalCheck)
		{
			if (DataCommand.Cash < 0m || DataCommand.ElectronicPayment < 0m || DataCommand.AdvancePayment < 0m || DataCommand.Credit < 0m || DataCommand.CashProvision < 0m)
			{
				throw new SystemException("Нельзя указывать отрицательные суммы в оплате");
			}
			decimal num3 = default(decimal);
			if (DataCommand.CheckStrings != null)
			{
				DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
				foreach (DataCommand.CheckString checkString7 in checkStrings)
				{
					if (DataCommand.IsFiscalCheck && checkString7 != null && checkString7.Register != null)
					{
						num3 += Math.Round(checkString7.Register.Amount, 2, MidpointRounding.ToEven);
						if (checkString7.Register.Quantity < 0m || checkString7.Register.Price < 0m || checkString7.Register.Amount < 0m)
						{
							throw new SystemException("Нельзя указывать отрицательные суммы, цены и количество для товара/услуги");
						}
						if (checkString7.Register.Quantity == 0m)
						{
							throw new SystemException("Нельзя указывать количество товара/услуги = 0");
						}
					}
				}
			}
			decimal num4 = Math.Round(DataCommand.Cash, 2, MidpointRounding.ToEven) + Math.Round(DataCommand.ElectronicPayment, 2, MidpointRounding.ToEven) + Math.Round(DataCommand.AdvancePayment, 2, MidpointRounding.ToEven) + Math.Round(DataCommand.Credit, 2, MidpointRounding.ToEven) + Math.Round(DataCommand.CashProvision, 2, MidpointRounding.ToEven);
			if (num4 < num3)
			{
				throw new SystemException("Сумма указанных оплат меньше чем сумма по строкам чека");
			}
			if (num4 >= num3 && num4 - num3 > DataCommand.Cash)
			{
				throw new SystemException("Сумма по чеку больше чем сумма по указанной оплате наличными - не с чего давать сдачу");
			}
		}
		if (DataCommand.IsFiscalCheck)
		{
			if (DataCommand.AgentData != null && (DataCommand.AgentData.MoneyTransferOperatorAddress == null || DataCommand.AgentData.MoneyTransferOperatorAddress == "") && (DataCommand.AgentData.MoneyTransferOperatorName == null || DataCommand.AgentData.MoneyTransferOperatorName == "") && (DataCommand.AgentData.MoneyTransferOperatorPhone == null || DataCommand.AgentData.MoneyTransferOperatorPhone == "") && (DataCommand.AgentData.MoneyTransferOperatorVATIN == null || DataCommand.AgentData.MoneyTransferOperatorVATIN == "") && (DataCommand.AgentData.PayingAgentOperation == null || DataCommand.AgentData.PayingAgentOperation == "") && (DataCommand.AgentData.PayingAgentPhone == null || DataCommand.AgentData.PayingAgentPhone == "") && (DataCommand.AgentData.ReceivePaymentsOperatorPhone == null || DataCommand.AgentData.ReceivePaymentsOperatorPhone == ""))
			{
				DataCommand.AgentData = null;
			}
			if (DataCommand.PurveyorData != null && (DataCommand.PurveyorData.PurveyorName == null || DataCommand.PurveyorData.PurveyorName == "") && (DataCommand.PurveyorData.PurveyorPhone == null || DataCommand.PurveyorData.PurveyorPhone == "") && (DataCommand.PurveyorData.PurveyorVATIN == null || DataCommand.PurveyorData.PurveyorVATIN == ""))
			{
				DataCommand.PurveyorData = null;
			}
			if (DataCommand.CheckStrings != null)
			{
				DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
				foreach (DataCommand.CheckString checkString8 in checkStrings)
				{
					if (DataCommand.IsFiscalCheck && checkString8 != null && checkString8.Register != null)
					{
						if (checkString8.Register.AgentData != null && (checkString8.Register.AgentData.MoneyTransferOperatorAddress == null || checkString8.Register.AgentData.MoneyTransferOperatorAddress == "") && (checkString8.Register.AgentData.MoneyTransferOperatorName == null || checkString8.Register.AgentData.MoneyTransferOperatorName == "") && (checkString8.Register.AgentData.MoneyTransferOperatorPhone == null || checkString8.Register.AgentData.MoneyTransferOperatorPhone == "") && (checkString8.Register.AgentData.MoneyTransferOperatorVATIN == null || checkString8.Register.AgentData.MoneyTransferOperatorVATIN == "") && (checkString8.Register.AgentData.PayingAgentOperation == null || checkString8.Register.AgentData.PayingAgentOperation == "") && (checkString8.Register.AgentData.PayingAgentPhone == null || checkString8.Register.AgentData.PayingAgentPhone == "") && (checkString8.Register.AgentData.ReceivePaymentsOperatorPhone == null || checkString8.Register.AgentData.ReceivePaymentsOperatorPhone == ""))
						{
							checkString8.Register.AgentData = null;
						}
						if (checkString8.Register.PurveyorData != null && (checkString8.Register.PurveyorData.PurveyorName == null || checkString8.Register.PurveyorData.PurveyorName == "") && (checkString8.Register.PurveyorData.PurveyorPhone == null || checkString8.Register.PurveyorData.PurveyorPhone == "") && (checkString8.Register.PurveyorData.PurveyorVATIN == null || checkString8.Register.PurveyorData.PurveyorVATIN == ""))
						{
							checkString8.Register.PurveyorData = null;
						}
					}
				}
			}
		}
		if (DataCommand.IsFiscalCheck)
		{
			if (DataCommand.AgentData != null)
			{
				if (!string.IsNullOrEmpty(DataCommand.AgentData.PayingAgentPhone))
				{
					DataCommand.AgentData.PayingAgentPhone = ParsePhone(DataCommand.AgentData.PayingAgentPhone);
				}
				if (!string.IsNullOrEmpty(DataCommand.AgentData.ReceivePaymentsOperatorPhone))
				{
					DataCommand.AgentData.ReceivePaymentsOperatorPhone = ParsePhone(DataCommand.AgentData.ReceivePaymentsOperatorPhone);
				}
				if (!string.IsNullOrEmpty(DataCommand.AgentData.MoneyTransferOperatorPhone))
				{
					DataCommand.AgentData.MoneyTransferOperatorPhone = ParsePhone(DataCommand.AgentData.MoneyTransferOperatorPhone);
				}
				if (!string.IsNullOrEmpty(DataCommand.AgentData.MoneyTransferOperatorVATIN) && !ChekINN(DataCommand.AgentData.MoneyTransferOperatorVATIN))
				{
					throw new SystemException("Некорректный ИНН агента");
				}
			}
			if (DataCommand.PurveyorData != null)
			{
				if (DataCommand.PurveyorData.PurveyorName == null || DataCommand.PurveyorData.PurveyorName == "")
				{
					throw new SystemException("В данных поставщика (клиента) не заполнено поле 'Наименование поставщика (клиента)'");
				}
				if (DataCommand.PurveyorData.PurveyorVATIN == null || DataCommand.PurveyorData.PurveyorVATIN == "")
				{
					throw new SystemException("В данных поставщика (клиента) не заполнено поле 'ИНН поставщика (клиента)'");
				}
				if (!string.IsNullOrEmpty(DataCommand.PurveyorData.PurveyorPhone))
				{
					DataCommand.PurveyorData.PurveyorPhone = ParsePhone(DataCommand.PurveyorData.PurveyorPhone);
				}
				if (DataCommand.PurveyorData.PurveyorVATIN != null && DataCommand.PurveyorData.PurveyorVATIN.Trim() != "" && DataCommand.PurveyorData.PurveyorVATIN.Trim().Length != 12 && DataCommand.PurveyorData.PurveyorVATIN.Trim().Length != 10)
				{
					throw new SystemException("ИНН поставщика должен содержать 10 или 12 символов");
				}
				if (!string.IsNullOrEmpty(DataCommand.PurveyorData.PurveyorVATIN) && !ChekINN(DataCommand.PurveyorData.PurveyorVATIN))
				{
					throw new SystemException("Некорректный ИНН поставщика");
				}
			}
			if (DataCommand.CheckStrings != null)
			{
				DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
				foreach (DataCommand.CheckString checkString9 in checkStrings)
				{
					if (!DataCommand.IsFiscalCheck || checkString9 == null || checkString9.Register == null)
					{
						continue;
					}
					if (checkString9.Register.AgentData != null)
					{
						if (!string.IsNullOrEmpty(checkString9.Register.AgentData.PayingAgentPhone))
						{
							checkString9.Register.AgentData.PayingAgentPhone = ParsePhone(checkString9.Register.AgentData.PayingAgentPhone);
						}
						if (!string.IsNullOrEmpty(checkString9.Register.AgentData.ReceivePaymentsOperatorPhone))
						{
							checkString9.Register.AgentData.ReceivePaymentsOperatorPhone = ParsePhone(checkString9.Register.AgentData.ReceivePaymentsOperatorPhone);
						}
						if (!string.IsNullOrEmpty(checkString9.Register.AgentData.MoneyTransferOperatorPhone))
						{
							checkString9.Register.AgentData.MoneyTransferOperatorPhone = ParsePhone(checkString9.Register.AgentData.MoneyTransferOperatorPhone);
						}
						if (!string.IsNullOrEmpty(checkString9.Register.AgentData.MoneyTransferOperatorVATIN) && !ChekINN(checkString9.Register.AgentData.MoneyTransferOperatorVATIN))
						{
							throw new SystemException("Некорректный ИНН агента");
						}
					}
					if (checkString9.Register.PurveyorData != null)
					{
						if (checkString9.Register.PurveyorData.PurveyorName == null || checkString9.Register.PurveyorData.PurveyorName == "")
						{
							throw new SystemException("В данных поставщика (клиента) не заполнено поле 'Наименование поставщика (клиента)'");
						}
						if (checkString9.Register.PurveyorData.PurveyorVATIN == null || checkString9.Register.PurveyorData.PurveyorVATIN == "")
						{
							throw new SystemException("В данных поставщика (клиента) не заполнено поле 'ИНН поставщика (клиента)'");
						}
						if (!string.IsNullOrEmpty(checkString9.Register.PurveyorData.PurveyorPhone))
						{
							checkString9.Register.PurveyorData.PurveyorPhone = ParsePhone(checkString9.Register.PurveyorData.PurveyorPhone);
						}
						if (checkString9.Register.PurveyorData.PurveyorVATIN != null && checkString9.Register.PurveyorData.PurveyorVATIN.Trim() != "" && checkString9.Register.PurveyorData.PurveyorVATIN.Trim().Length != 12 && checkString9.Register.PurveyorData.PurveyorVATIN.Trim().Length != 10)
						{
							throw new SystemException("ИНН поставщика должен содержать 10 или 12 символов");
						}
						if (!string.IsNullOrEmpty(checkString9.Register.PurveyorData.PurveyorVATIN) && !ChekINN(checkString9.Register.PurveyorData.PurveyorVATIN))
						{
							throw new SystemException("Некорректный ИНН поставщика");
						}
					}
				}
			}
		}
		if (DataCommand.IsFiscalCheck)
		{
			bool flag = DataCommand.Cash != 0m || DataCommand.ElectronicPayment != 0m || DataCommand.AdvancePayment != 0m || DataCommand.CashProvision != 0m;
			bool flag2 = DataCommand.Credit != 0m;
			DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
			foreach (DataCommand.CheckString checkString10 in checkStrings)
			{
				if (checkString10.Register != null)
				{
					if (flag2 && !flag && checkString10.Register.SignMethodCalculation == 4)
					{
						checkString10.Register.SignMethodCalculation = 6;
					}
					else if (flag2 && flag && checkString10.Register.SignMethodCalculation == 4)
					{
						checkString10.Register.SignMethodCalculation = 5;
					}
					else if (checkString10.Register.SignMethodCalculation == 5 && !flag2)
					{
						checkString10.Register.SignMethodCalculation = 4;
					}
					else if (checkString10.Register.SignMethodCalculation == 6 && !flag2)
					{
						checkString10.Register.SignMethodCalculation = 4;
					}
				}
			}
		}
		if (!DataCommand.IsFiscalCheck)
		{
			DataCommand.GoodCodeDatas = null;
			DataCommand.CashierName = null;
			DataCommand.CashierVATIN = null;
			DataCommand.ClientAddress = null;
			DataCommand.ClientInfo = null;
			DataCommand.ClientINN = null;
			DataCommand.SenderEmail = null;
			DataCommand.AddressSettle = null;
			DataCommand.PlaceMarket = null;
			DataCommand.AgentSign = null;
			DataCommand.AgentData = null;
			DataCommand.PurveyorData = null;
			DataCommand.UserAttribute = null;
			DataCommand.AdditionalAttribute = null;
			DataCommand.Cash = default(decimal);
			DataCommand.ElectronicPayment = default(decimal);
			DataCommand.AdvancePayment = default(decimal);
			DataCommand.Credit = default(decimal);
			DataCommand.CashProvision = default(decimal);
			DataCommand.CorrectionBaseDate = null;
			DataCommand.CorrectionBaseNumber = null;
			DataCommand.Amount = default(decimal);
			DataCommand.TaxVariant = null;
			DataCommand.CheckProps = null;
			DataCommand.AdditionalProps = null;
			DataCommand.RegKkmOfd = null;
		}
		if (DataCommand.IsFiscalCheck && ((DataCommand.TaxVariant == "") & (Kkm.TaxVariant.Trim() == "")))
		{
			RezultCommandKKm rezultCommandKKm = new RezultCommandKKm();
			rezultCommandKKm.RunComPort = true;
			DataCommand dataCommand = new DataCommand();
			dataCommand.Command = "GetDataKKT";
			dataCommand.NumDevice = DataCommand.NumDevice;
			dataCommand.KktNumber = DataCommand.KktNumber;
			dataCommand.InnKkm = DataCommand.InnKkm;
			dataCommand.TaxVariant = DataCommand.TaxVariant;
			dataCommand.IdCommand = Guid.NewGuid().ToString();
			dataCommand.RunComPort = true;
			NetLogs.Append("\r\n\r\nНет TaxVariant, запрашиваем - GetDataKKT");
			await ProcessGetDataKKT(dataCommand, rezultCommandKKm);
			NetLogs.Append("\r\n\r\nTaxVariant = " + Kkm.TaxVariant.Trim());
		}
	}

	private string ParsePhone(string s)
	{
		char[] array = new char[11]
		{
			'+', '0', '1', '2', '3', '4', '5', '6', '7', '8',
			'9'
		};
		string text = "";
		foreach (char c in s)
		{
			if (array.Contains(c))
			{
				text += c;
			}
		}
		if (text[0] != '+')
		{
			if (text.Length != 10)
			{
				throw new SystemException("Не правильный формат телефона: '" + s + "'! Правильный формат '+ЦЦЦЦЦЦЦЦЦЦ'");
			}
			text = "+" + text;
		}
		return text;
	}

	public int GetCodeChangeKkmReg(DataCommand.TypeRegKkmOfd RegKkmOfd)
	{
		int num = 0;
		if (RegKkmOfd.Command == "Open")
		{
			return num;
		}
		if (RegKkmOfd.Command == "ChangeFN")
		{
			num |= 1;
		}
		if (RegKkmOfd.Command == "ChangeOFD")
		{
			if (Kkm.InnOfd.Trim() != RegKkmOfd.InnOfd.Trim())
			{
				num |= 2;
			}
			if (Kkm.NameOFD.Trim() != RegKkmOfd.NameOFD.Trim())
			{
				num |= int.MinValue;
			}
		}
		if (RegKkmOfd.Command == "ChangeOrganization")
		{
			if (RegKkmOfd.NameOrganization.Trim() != Kkm.Organization.Trim())
			{
				num |= 4;
			}
			if (RegKkmOfd.AddressSettle.Trim() != Kkm.AddressSettle.Trim())
			{
				num |= 8;
			}
			if (RegKkmOfd.PlaceSettle.Trim() != Kkm.PlaceSettle.Trim())
			{
				num |= 8;
			}
			if (RegKkmOfd.TaxVariant.Trim() != Kkm.TaxVariant.Trim())
			{
				num |= 0x80;
			}
			if (Kkm.SignOfAgent.Trim() == "" && RegKkmOfd.SignOfAgent.Trim() != Kkm.SignOfAgent.Trim())
			{
				num |= 0x10000;
			}
			if (RegKkmOfd.SignOfAgent.Trim() == "" && RegKkmOfd.SignOfAgent.Trim() != Kkm.SignOfAgent.Trim())
			{
				num |= 0x8000;
			}
		}
		if (RegKkmOfd.Command == "ChangeKkm")
		{
			if (Kkm.OfflineMode && !RegKkmOfd.OfflineMode)
			{
				num |= 0x10;
			}
			if (!Kkm.OfflineMode && RegKkmOfd.OfflineMode)
			{
				num |= 0x20;
			}
			if (Kkm.EncryptionMode != RegKkmOfd.EncryptionMode)
			{
				num |= int.MinValue;
			}
			if (Kkm.ServiceMode != RegKkmOfd.ServiceMode)
			{
				num |= int.MinValue;
			}
			if (Kkm.BSOMode && !RegKkmOfd.BSOMode)
			{
				num |= 0x1000;
			}
			if (!Kkm.BSOMode && RegKkmOfd.BSOMode)
			{
				num |= 0x800;
			}
			if (Kkm.AutomaticMode && !RegKkmOfd.AutomaticMode)
			{
				num |= 0x200;
			}
			if (!Kkm.AutomaticMode && RegKkmOfd.AutomaticMode)
			{
				num |= 0x400;
			}
			if (Kkm.InternetMode && !RegKkmOfd.InternetMode)
			{
				num |= 0x2000;
			}
			if (!Kkm.InternetMode && RegKkmOfd.InternetMode)
			{
				num |= 0x4000;
			}
			if (Kkm.PrinterAutomatic != RegKkmOfd.PrinterAutomatic)
			{
				num |= int.MinValue;
			}
			if (Kkm.SignOfGambling && !RegKkmOfd.SignOfGambling)
			{
				num |= 0x20000;
			}
			if (!Kkm.SignOfGambling && RegKkmOfd.SignOfGambling)
			{
				num |= 0x40000;
			}
			if (Kkm.SignOfLottery && !RegKkmOfd.SignOfLottery)
			{
				num |= 0x80000;
			}
			if (!Kkm.SignOfLottery && RegKkmOfd.SignOfLottery)
			{
				num |= 0x100000;
			}
			if (Kkm.SaleExcisableGoods != RegKkmOfd.SaleExcisableGoods)
			{
				num |= int.MinValue;
			}
			if (Kkm.AutomaticNumber != RegKkmOfd.AutomaticNumber)
			{
				num |= int.MinValue;
			}
		}
		if ((RegKkmOfd.Command == "ChangeFN" || RegKkmOfd.Command == "ChangeKkm") && RegKkmOfd.SetFfdVersion != Kkm.FfdVersion)
		{
			num |= 0x200000;
		}
		return num;
	}

	public static string GetUrlFromQRCode(string QRCode, string InnOfd, string Inn, string Rn)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (InnOfd == "")
		{
			return "";
		}
		if (QRCode == "")
		{
			return "";
		}
		string[] array = QRCode.Split(new char[1] { '&' }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[1] { '=' }, StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length == 2)
			{
				dictionary.Add(array2[0], array2[1]);
			}
		}
		string text = Rn;
		if (text.Length > 16)
		{
			text = text.Substring(text.Length - 16, 16);
		}
		string text2 = "";
		switch (InnOfd.Trim())
		{
		case "7709364346":
			text2 = "https://consumer.1-ofd.ru/ticket?t={tyyyy}{tMM}{tdd}T{tHH}{tmm}&s={s}&fn={fn}&i={i}&fp={fp}&n={n}";
			break;
		case "7704211201":
			text2 = "https://receipt.taxcom.ru/v01/show?fp={fp}&s={s}";
			break;
		case "9715260691":
			text2 = "https://lk.platformaofd.ru/web/noauth/cheque/search?fn={fn}&i={i}&fp={fp}";
			break;
		case "7728699517":
			text2 = " https://ofd-ya.ru/check?fiscalDriveNumber={fn}&fiscalDocumentNumber={i}&fiscalSign={fp}";
			break;
		case "7841465198":
			text2 = "https://check.ofd.ru/rec/{inn}/{rn1}/{fn}/{i}/{fp}";
			break;
		case "7704358518":
			text2 = "https://ofd.yandex.ru/vaucher/{rn}/{i}/{fp}";
			break;
		case "7729633131":
			text2 = "";
			break;
		case "4029017981":
			text2 = "http://ofd.astralnalog.ru/";
			break;
		case "7605016030":
			text2 = "https://ofd.sbis.ru/rec/{rn1}/{tyyyy}-{tMM}-{tdd}%20{tHH}:{tmm}:00/{fp}";
			break;
		case "7801392271":
			text2 = "https://www.esphere.ru/products/ofd";
			break;
		case "6663003127":
			text2 = "https://cash.kontur.ru/CashReceipt/View/FN/{fn}/FD/{i}/FP/{fp}";
			break;
		case "6658497833":
			text2 = "https://cash-ntt.kontur.ru/?fnSerialNumber={fn}&fiscalDocumentNumber={i}&fiscalSignature={fp}";
			break;
		case "2310031475":
			text2 = "";
			break;
		}
		string text3 = (dictionary.ContainsKey("s") ? dictionary["t"] : "00000000T000000");
		text2 = text2.Replace("{fn}", dictionary.ContainsKey("fn") ? dictionary["fn"] : "{НеНайдено}");
		text2 = text2.Replace("{fp}", dictionary.ContainsKey("fp") ? dictionary["fp"] : "{НеНайдено}");
		text2 = text2.Replace("{i}", dictionary.ContainsKey("i") ? dictionary["i"] : "{НеНайдено}");
		text2 = text2.Replace("{rn}", Rn.ToString());
		text2 = text2.Replace("{rn1}", text.ToString());
		text2 = text2.Replace("{n}", dictionary.ContainsKey("n") ? dictionary["n"] : "{НеНайдено}");
		text2 = text2.Replace("{s}", dictionary.ContainsKey("s") ? dictionary["s"] : "{НеНайдено}");
		text2 = text2.Replace("{DateTime}", text3);
		text2 = text2.Replace("{tddmmyy}", text3.Substring(6, 2) + text3.Substring(4, 2) + text3.Substring(2, 2));
		text2 = text2.Replace("{tyyyy}", text3.Substring(0, 4));
		text2 = text2.Replace("{tMM}", text3.Substring(4, 2));
		text2 = text2.Replace("{tdd}", text3.Substring(6, 2));
		text2 = text2.Replace("{tHH}", text3.Substring(9, 2));
		text2 = text2.Replace("{tmm}", text3.Substring(11, 2));
		text2 = text2.Replace("{inn}", Inn.ToString());
		if (text2.IndexOf("{НеНайдено>") != -1)
		{
			text2 = "";
		}
		return text2;
	}

	public virtual async Task<uint> GetLastFiscalNumber()
	{
		return 0u;
	}

	public virtual async Task<Dictionary<int, string>> GetRegisterCheck(uint FiscalNumber, Dictionary<int, Type> Types)
	{
		return null;
	}

	public async Task GetDataCheck(DataCommand DataCommand, RezultCommandCheck RezultCommand)
	{
		Dictionary<int, Type> Types = new Dictionary<int, Type>
		{
			{
				1054,
				typeof(int)
			},
			{
				1020,
				typeof(decimal)
			},
			{
				1012,
				typeof(DateTime)
			},
			{
				1077,
				typeof(uint)
			},
			{
				1040,
				typeof(uint)
			},
			{
				1021,
				typeof(string)
			},
			{
				1203,
				typeof(string)
			},
			{
				1038,
				typeof(uint)
			},
			{
				1042,
				typeof(uint)
			},
			{
				1117,
				typeof(string)
			},
			{
				1008,
				typeof(string)
			},
			{
				1187,
				typeof(string)
			},
			{
				1031,
				typeof(decimal)
			},
			{
				1081,
				typeof(decimal)
			},
			{
				1215,
				typeof(decimal)
			},
			{
				1216,
				typeof(decimal)
			},
			{
				1217,
				typeof(decimal)
			},
			{
				1023,
				typeof(decimal)
			},
			{
				1043,
				typeof(decimal)
			},
			{
				1030,
				typeof(string)
			},
			{
				1055,
				typeof(byte)
			},
			{
				1199,
				typeof(byte)
			},
			{
				1162,
				typeof(byte[])
			}
		};
		uint num = ((DataCommand.FiscalNumber.HasValue && DataCommand.FiscalNumber != 0) ? ((uint)DataCommand.FiscalNumber.Value) : (await GetLastFiscalNumber()));
		if (num == 0)
		{
			if (Error == "")
			{
				Error = "Не удалось получить номер последнего документа";
			}
			return;
		}
		Dictionary<int, string> RegisterChecDic;
		try
		{
			RegisterChecDic = await GetRegisterCheck(num, Types);
		}
		catch (Exception)
		{
			RegisterChecDic = null;
		}
		if (RegisterChecDic == null)
		{
			if (Error == "")
			{
				Error = "Не удалось получить данные чека";
			}
			return;
		}
		RezultCommandCheck.RegisterCheck RegisterCheck = new RezultCommandCheck.RegisterCheck
		{
			FiscalNumber = (RegisterChecDic.ContainsKey(1040) ? RegisterChecDic[1040] : ""),
			FiscalDate = (RegisterChecDic.ContainsKey(1012) ? RegisterChecDic[1012].AsDateTime() : default(DateTime)),
			FiscalSign = (RegisterChecDic.ContainsKey(1077) ? RegisterChecDic[1077] : ""),
			AllSumm = (RegisterChecDic.ContainsKey(1020) ? RegisterChecDic[1020].AsDecimal() : 0m)
		};
		if (RegisterChecDic[0].AsInt() == 3)
		{
			if (!RegisterChecDic.ContainsKey(1054))
			{
				RegisterCheck.CheckType = "Чек";
			}
			else
			{
				switch (RegisterChecDic[1054].AsInt())
				{
				case 1:
					RegisterCheck.CheckType = "Приход";
					break;
				case 2:
					RegisterCheck.CheckType = "Возврат прихода";
					break;
				case 3:
					RegisterCheck.CheckType = "Расход";
					break;
				case 4:
					RegisterCheck.CheckType = "Возврат расхода";
					break;
				}
			}
		}
		else
		{
			switch (RegisterChecDic[0].AsInt())
			{
			case 31:
				if (!RegisterChecDic.ContainsKey(1054))
				{
					RegisterCheck.CheckType = "Коррекция";
				}
				else if (RegisterChecDic[1054].AsInt() == 1)
				{
					RegisterCheck.CheckType = "Коррекция приход";
				}
				else if (RegisterChecDic[1054].AsInt() == 3)
				{
					RegisterCheck.CheckType = "Коррекция расход";
				}
				break;
			case 1:
				RegisterCheck.CheckType = "Отчет о регистрации";
				break;
			case 11:
				RegisterCheck.CheckType = "Отчет об изменении параметров регистрации";
				break;
			case 2:
				RegisterCheck.CheckType = "Открытие смены";
				break;
			case 21:
				RegisterCheck.CheckType = "Текущее состояние расчетов";
				break;
			case 4:
				RegisterCheck.CheckType = "Бланк строгой отчетности";
				break;
			case 41:
				RegisterCheck.CheckType = "Бланк строгой отчетности коррекции";
				break;
			case 5:
				RegisterCheck.CheckType = "Закрытие смены";
				break;
			case 6:
				RegisterCheck.CheckType = "Отчет о закрытии фискального накопителя";
				break;
			case 7:
				RegisterCheck.CheckType = "Подтверждение оператора";
				break;
			}
		}
		RegisterCheck.CashierName = (RegisterChecDic.ContainsKey(1021) ? RegisterChecDic[1021] : "");
		RegisterCheck.CashierVATIN = (RegisterChecDic.ContainsKey(1203) ? RegisterChecDic[1203] : "");
		RegisterCheck.ClientAddress = (RegisterChecDic.ContainsKey(1008) ? RegisterChecDic[1008] : "");
		RegisterCheck.SenderEmail = (RegisterChecDic.ContainsKey(1117) ? RegisterChecDic[1117] : Kkm.SenderEmail);
		RegisterCheck.PlaceMarket = (RegisterChecDic.ContainsKey(1187) ? RegisterChecDic[1187] : Kkm.PlaceSettle);
		RegisterCheck.TaxVariant = (RegisterChecDic.ContainsKey(1055) ? RegisterChecDic[1055] : "");
		RegisterCheck.Cash = (RegisterChecDic.ContainsKey(1031) ? RegisterChecDic[1031].AsDecimal() : 0m);
		RegisterCheck.ElectronicPayment = (RegisterChecDic.ContainsKey(1081) ? RegisterChecDic[1081].AsDecimal() : 0m);
		RegisterCheck.AdvancePayment = (RegisterChecDic.ContainsKey(1215) ? RegisterChecDic[1215].AsDecimal() : 0m);
		RegisterCheck.Credit = (RegisterChecDic.ContainsKey(1216) ? RegisterChecDic[1216].AsDecimal() : 0m);
		RegisterCheck.CashProvision = (RegisterChecDic.ContainsKey(1217) ? RegisterChecDic[1217].AsDecimal() : 0m);
		if (RegisterChecDic.ContainsKey(1055))
		{
			byte b = RegisterChecDic[1055].AsByte();
			for (int i = 0; i <= 5; i++)
			{
				if (((b >> i) & 1) == 1)
				{
					RegisterCheck.TaxVariant = i.ToString();
				}
			}
		}
		if (RegisterChecDic.ContainsKey(1059))
		{
			List<RezultCommandCheck.Register> list = new List<RezultCommandCheck.Register>();
			foreach (Dictionary<int, string> item in RegisterChecDic[1059].AsListDictionaryIntString())
			{
				RezultCommandCheck.Register register = new RezultCommandCheck.Register();
				register.Amount = (item.ContainsKey(1043) ? item[1043].AsDecimal() : 0m);
				register.Quantity = (item.ContainsKey(1023) ? item[1023].AsDecimal() : 0m);
				register.Tax = (item.ContainsKey(1199) ? item[1199].AsDecimal() : 0m);
				decimal tax = register.Tax;
				if (tax <= 5m)
				{
					if (tax <= 2m)
					{
						if (!(tax == 1m))
						{
							if (tax == 2m)
							{
								register.Tax = 10m;
							}
						}
						else
						{
							register.Tax = 22m;
						}
					}
					else if (!(tax == 3m))
					{
						if (!(tax == 4m))
						{
							if (tax == 5m)
							{
								register.Tax = default(decimal);
							}
						}
						else
						{
							register.Tax = 110m;
						}
					}
					else
					{
						register.Tax = 122m;
					}
				}
				else if (tax <= 7m)
				{
					if (!(tax == 6m))
					{
						if (tax == 7m)
						{
							register.Tax = 5m;
						}
					}
					else
					{
						register.Tax = -1m;
					}
				}
				else if (!(tax == 8m))
				{
					if (!(tax == 9m))
					{
						if (tax == 10m)
						{
							register.Tax = 107m;
						}
					}
					else
					{
						register.Tax = 105m;
					}
				}
				else
				{
					register.Tax = 7m;
				}
				register.Name = (item.ContainsKey(1030) ? item[1030] : "<Не указано>");
				if (item.ContainsKey(1162))
				{
					register.GoodCodeData = new DataCommand.Register.tGoodCodeData();
					BinaryReader binaryReader = new BinaryReader(new MemoryStream(item[1162].AsArrayByte()));
					byte[] array = binaryReader.ReadBytes(2);
					Array.Reverse(array);
					register.GoodCodeData.StampType = BitConverter.ToUInt16(array, 0).ToString("X").PadLeft(2, '0');
					array = binaryReader.ReadBytes(6);
					Array.Reverse(array);
					Array.Resize(ref array, 8);
					register.GoodCodeData.GTIN = BitConverter.ToUInt64(array, 0).ToString();
					register.GoodCodeData.SerialNumber = Encoding.GetEncoding(866).GetString(binaryReader.ReadBytes(100));
				}
				list.Add(register);
			}
			RegisterCheck.Register = list.ToArray();
		}
		RezultCommand.Check = RegisterCheck;
		string text = "";
		text = text + GetPringString("<<->>", Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString("ИНН организации:<#0#>" + Kkm.INN, Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString(Kkm.Organization + ", " + Kkm.AddressSettle, Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString("Заводской номер ККТ:<#0#>" + Kkm.NumberKkm, Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString("<<->>", Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString("Тип чека:<#0#>" + RegisterCheck.CheckType, Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString("Кассир:<#0#>" + RegisterCheck.CashierName, Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString("ИНН кассира:<#0#>" + RegisterCheck.CashierVATIN, Kkm.PrintingWidth) + "\r\n";
		if (RegisterCheck.TaxVariant != "")
		{
			string text2 = "";
			switch (RegisterCheck.TaxVariant)
			{
			case "0":
				text2 = "Общая (ОСН)";
				break;
			case "1":
				text2 = "УСН (Доход)";
				break;
			case "2":
				text2 = "УСН (Доход-Расход)";
				break;
			case "3":
				text2 = "ЕНВД";
				break;
			case "4":
				text2 = "ЕСН";
				break;
			case "5":
				text2 = "Патент";
				break;
			}
			text = text + GetPringString("Система налогообложения:<#0#>" + text2, Kkm.PrintingWidth) + "\r\n";
		}
		if (RegisterCheck.ClientAddress != "")
		{
			text = text + GetPringString("Адрес покупателя:<#0#>" + RegisterCheck.ClientAddress, Kkm.PrintingWidth) + "\r\n";
		}
		if (RegisterCheck.SenderEmail != "")
		{
			text = text + GetPringString("Адрес отправителя:<#0#>" + RegisterCheck.SenderEmail, Kkm.PrintingWidth) + "\r\n";
		}
		if (RegisterCheck.PlaceMarket != "")
		{
			text = text + GetPringString("Место расчетов:<#0#>" + RegisterCheck.PlaceMarket, Kkm.PrintingWidth) + "\r\n";
		}
		text = text + GetPringString("<<->>", Kkm.PrintingWidth) + "\r\n";
		if (RegisterCheck.Register != null)
		{
			RezultCommandCheck.Register[] register2 = RegisterCheck.Register;
			foreach (RezultCommandCheck.Register register3 in register2)
			{
				text = text + GetPringString(register3.Name, Kkm.PrintingWidth) + "\r\n";
				text = text + GetPringString("Количество:<#16#>>" + register3.Quantity, Kkm.PrintingWidth) + "\r\n";
				text = text + GetPringString("Сумма:<#16#>>" + register3.Amount, Kkm.PrintingWidth) + "\r\n";
				string text3 = "";
				decimal num2 = default(decimal);
				decimal tax = register3.Tax;
				if (tax <= 20m)
				{
					if (tax <= 5m)
					{
						if (!(tax == -1m))
						{
							if (!(tax == 0m))
							{
								if (tax == 5m)
								{
									text3 = "НДС 5%:";
									num2 = Math.Round(register3.Amount / 105m * 5m, 2, MidpointRounding.AwayFromZero);
								}
							}
							else
							{
								text3 = "НДС 0%:";
							}
						}
						else
						{
							text3 = "Без НДС:";
						}
					}
					else if (tax <= 10m)
					{
						if (!(tax == 7m))
						{
							if (tax == 10m)
							{
								text3 = "НДС 10%:";
								num2 = Math.Round(register3.Amount / 110m * 10m, 2, MidpointRounding.AwayFromZero);
							}
						}
						else
						{
							text3 = "НДС 7%:";
							num2 = Math.Round(register3.Amount / 107m * 7m, 2, MidpointRounding.AwayFromZero);
						}
					}
					else if (!(tax == 18m))
					{
						if (tax == 20m)
						{
							text3 = "НДС 20%:";
							num2 = Math.Round(register3.Amount / 120m * 20m, 2, MidpointRounding.AwayFromZero);
						}
					}
					else
					{
						text3 = "НДС 18%:";
						num2 = Math.Round(register3.Amount / 118m * 18m, 2, MidpointRounding.AwayFromZero);
					}
				}
				else if (tax <= 107m)
				{
					if (!(tax == 22m))
					{
						if (!(tax == 105m))
						{
							if (tax == 107m)
							{
								text3 = "НДС 7/107:%";
								num2 = Math.Round(register3.Amount / 107m * 7m, 2, MidpointRounding.AwayFromZero);
							}
						}
						else
						{
							text3 = "НДС 5/105:%";
							num2 = Math.Round(register3.Amount / 105m * 5m, 2, MidpointRounding.AwayFromZero);
						}
					}
					else
					{
						text3 = "НДС 22%:";
						num2 = Math.Round(register3.Amount / 122m * 22m, 2, MidpointRounding.AwayFromZero);
					}
				}
				else if (tax <= 118m)
				{
					if (!(tax == 110m))
					{
						if (tax == 118m)
						{
							text3 = "НДС 18/118:%";
							num2 = Math.Round(register3.Amount / 118m * 18m, 2, MidpointRounding.AwayFromZero);
						}
					}
					else
					{
						text3 = "НДС 10/110%:";
						num2 = Math.Round(register3.Amount / 110m * 10m, 2, MidpointRounding.AwayFromZero);
					}
				}
				else if (!(tax == 120m))
				{
					if (tax == 122m)
					{
						text3 = "НДС 22/122:%";
						num2 = Math.Round(register3.Amount / 122m * 22m, 2, MidpointRounding.AwayFromZero);
					}
				}
				else
				{
					text3 = "НДС 20/120:%";
					num2 = Math.Round(register3.Amount / 120m * 20m, 2, MidpointRounding.AwayFromZero);
				}
				text = text + GetPringString(text3 + "<#16#>>" + num2, Kkm.PrintingWidth) + "\r\n";
				if (register3.GoodCodeData != null)
				{
					text = text + GetPringString("КИЗ:<#0#>" + register3.GoodCodeData.StampType + "(01)" + register3.GoodCodeData.GTIN + "(21)" + register3.GoodCodeData.SerialNumber, Kkm.PrintingWidth) + "\r\n";
				}
				text = text + GetPringString("<<->>", Kkm.PrintingWidth) + "\r\n";
			}
		}
		if (RegisterCheck.AllSumm == 3m || RegisterChecDic[0].AsInt() == 3)
		{
			text = text + GetPringString("ИТОГ:<#0#>" + RegisterCheck.AllSumm, Kkm.PrintingWidth) + "\r\n";
			text = text + GetPringString("Получено:<#0#>", Kkm.PrintingWidth) + "\r\n";
			if (RegisterCheck.Cash != 0m)
			{
				text = text + GetPringString("Наличными:<#0#>" + RegisterCheck.Cash, Kkm.PrintingWidth) + "\r\n";
			}
			if (RegisterCheck.ElectronicPayment != 0m)
			{
				text = text + GetPringString("Электронно:<#0#>" + RegisterCheck.ElectronicPayment, Kkm.PrintingWidth) + "\r\n";
			}
			if (RegisterCheck.AdvancePayment != 0m)
			{
				text = text + GetPringString("Зачет аванса:<#0#>" + RegisterCheck.AdvancePayment, Kkm.PrintingWidth) + "\r\n";
			}
			if (RegisterCheck.Credit != 0m)
			{
				text = text + GetPringString("Кредит:<#0#>" + RegisterCheck.Credit, Kkm.PrintingWidth) + "\r\n";
			}
			if (RegisterCheck.CashProvision != 0m)
			{
				text = text + GetPringString("Встречное представление:<#0#>" + DataCommand.CashProvision, Kkm.PrintingWidth) + "\r\n";
			}
			text = text + GetPringString("<<->>", Kkm.PrintingWidth) + "\r\n";
		}
		text = text + GetPringString("Номер ФД:<#0#>" + RegisterCheck.FiscalNumber.ToString(), Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString("Дата:<#0#>" + RegisterCheck.FiscalDate, Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString("Подпись ФПД:<#0#>" + RegisterCheck.FiscalSign.ToString(), Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString("Номер ФН:<#0#>" + Kkm.Fn_Number, Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString("Рег. #:<#0#>" + Kkm.RegNumber, Kkm.PrintingWidth) + "\r\n";
		text = text + GetPringString("<<->>", Kkm.PrintingWidth) + "\r\n";
		RezultCommand.Slip = FormatSlipCheck(text);
		if (DataCommand.NumberCopies != 0)
		{
			await PringArrayString(text, DataCommand.NumberCopies);
		}
		RezultCommand.Status = ExecuteStatus.Ok;
		if (RegisterChecDic[0].AsInt() == 3)
		{
			if (RegisterChecDic.ContainsKey(1054))
			{
				RezultCommand.QRCode = "t=" + RegisterCheck.FiscalDate.ToString("yyyyMMddTHHmm") + "&s=" + RegisterCheck.AllSumm.ToString("0.00").Replace(',', '.') + "&fn=" + Kkm.Fn_Number + "&i=" + RegisterCheck.FiscalNumber + "&fp=" + RegisterCheck.FiscalSign + "&n=" + RegisterChecDic[1054].ToString();
				RezultCommand.URL = GetUrlFromQRCode(RezultCommand.QRCode, Kkm.InnOfd, Kkm.INN, Kkm.RegNumber);
			}
		}
		else if (RegisterChecDic[0].AsInt() == 1 && RegisterChecDic[0].AsInt() == 2 && RegisterChecDic[0].AsInt() == 3 && RegisterChecDic[0].AsInt() == 4 && RegisterChecDic[0].AsInt() == 11 && RegisterChecDic[0].AsInt() == 31)
		{
			RezultCommand.QRCode = "t=" + RegisterCheck.FiscalDate.ToString("yyyyMMddTHHmm") + "&fn=" + Kkm.Fn_Number + "&i=" + RegisterCheck.FiscalNumber + "&fp=" + RegisterCheck.FiscalSign;
			RezultCommand.URL = GetUrlFromQRCode(RezultCommand.QRCode, Kkm.InnOfd, Kkm.INN, Kkm.RegNumber);
		}
	}

	public async Task ProcessGetDataCheck(DataCommand DataCommand, RezultCommandCheck RezultCommand)
	{
		RezultCommandKKm rezultCommand = null;
		bool flag = ProcessRoute(DataCommand, rezultCommand, RezultCommand);
		if (!SettDr.Paramets["EmulationCheck"].AsBool() || !flag)
		{
			await GetDataCheck(DataCommand, RezultCommand);
		}
		else if (DataCommand.NumberCopies != 0)
		{
			await PringArrayString(RezultCommand.Slip, DataCommand.NumberCopies);
		}
	}

	public virtual async Task GetCounters(DataCommand DataCommand, RezultCounters RezultCommand)
	{
	}

	public async Task ProcessGetCounters(DataCommand DataCommand, RezultCounters RezultCommand)
	{
		if (Kkm.FfdVersion < 3)
		{
			Error = "ККТ не поддерживает формат ФФД 1.1 и выше";
		}
		else
		{
			await GetCounters(DataCommand, RezultCommand);
		}
	}

	public static ulong NumberFromArray(byte[] Data, int Pos, int CountByte)
	{
		decimal num = default(decimal);
		decimal num2 = 1m;
		for (int i = Pos; i < Pos + CountByte; i++)
		{
			num += (decimal)Data[i] * num2;
			num2 *= 256m;
		}
		return (ulong)num;
	}

	public static byte[] GetData1162(DataCommand.Register.tGoodCodeData GoodCodeData)
	{
		MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		binaryWriter.Write(HexStringToByteArray(GoodCodeData.StampType.PadLeft(4, '0')));
		binaryWriter.Write(HexStringToByteArray(ulong.Parse(GoodCodeData.GTIN).ToString("X12")));
		binaryWriter.Write(Encoding.GetEncoding(866).GetBytes(GoodCodeData.SerialNumber));
		return memoryStream.ToArray();
	}

	public void SaveParemeterSearhKKT()
	{
		bool flag = false;
		if (!string.IsNullOrEmpty(Kkm.INN) && Kkm.INN != SettDr.Paramets["Save_INN"])
		{
			SettDr.Paramets["Save_INN"] = Kkm.INN;
			flag = true;
		}
		if (!string.IsNullOrEmpty(Kkm.NumberKkm) && Kkm.NumberKkm != SettDr.Paramets["Save_NumberKkm"])
		{
			SettDr.Paramets["Save_NumberKkm"] = Kkm.NumberKkm;
			flag = true;
		}
		if (!string.IsNullOrEmpty(Kkm.TaxVariant) && Kkm.NumberKkm != SettDr.Paramets["Save_TaxVariant"])
		{
			SettDr.Paramets["Save_TaxVariant"] = Kkm.TaxVariant;
			flag = true;
		}
	}

	public static byte[] HexStringToByteArray(string hex)
	{
		return (from x in Enumerable.Range(0, hex.Length)
			where x % 2 == 0
			select Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
	}

	public List<DataCommand.Register> SplitRegisterString(DataCommand.CheckString PrintString)
	{
		return SplitRegisterString(PrintString, this);
	}

	public static List<DataCommand.Register> SplitRegisterString(DataCommand.CheckString PrintString, Unit Unit)
	{
		List<DataCommand.Register> list = new List<DataCommand.Register>();
		decimal num = Math.Round(PrintString.Register.Quantity * PrintString.Register.Price, 2, MidpointRounding.ToEven) - PrintString.Register.Amount;
		num = Math.Round(num, 2, MidpointRounding.ToEven);
		decimal num2 = Math.Round(PrintString.Register.Price, 2, MidpointRounding.ToEven);
		decimal num3 = Math.Round(PrintString.Register.Amount / PrintString.Register.Quantity, 2, MidpointRounding.ToEven);
		if (Math.Round(num3 * PrintString.Register.Quantity, 2, MidpointRounding.ToEven) > PrintString.Register.Amount)
		{
			num3 -= 0.01m;
		}
		decimal num4 = PrintString.Register.Amount;
		if (PrintString.Register.Quantity > 1m)
		{
			num4 = Math.Round(num3 * PrintString.Register.Quantity, 2, MidpointRounding.ToEven);
		}
		decimal num5 = PrintString.Register.Quantity;
		decimal num6 = default(decimal);
		decimal num7 = default(decimal);
		decimal num8 = default(decimal);
		if (num4 - PrintString.Register.Amount != 0m && PrintString.Register.Quantity > 1m)
		{
			num5 -= 1m;
			num4 = Math.Round(num3 * num5, 2, MidpointRounding.ToEven);
			num8 = 1m;
			num6 = PrintString.Register.Amount - num4;
			num7 = Math.Round(num6 * num8, 2, MidpointRounding.ToEven);
		}
		for (int i = 0; i <= 1; i++)
		{
			if (i == 1)
			{
				num5 = num8;
				num4 = num7;
				num3 = num6;
			}
			if (!(num3 == 0m) || !(num5 == 0m))
			{
				if (i == 1)
				{
					num3 = Math.Round(num4 / num5, 2, MidpointRounding.ToEven);
				}
				string stSkidka = "";
				int len = 32;
				if (Unit != null)
				{
					len = Unit.Kkm.PrintingWidth;
				}
				if (num > 0m)
				{
					stSkidka = GetPringString("Скидка:<#0#>" + (-num4 + num2 * num5), len);
				}
				else if (num < 0m)
				{
					stSkidka = GetPringString("Наценка:<#0#>" + (num4 - num2 * num5), len);
				}
				DataCommand.Register register = new DataCommand.Register();
				register.Quantity = num5;
				register.Price = num3;
				register.Amount = num4;
				register.Department = PrintString.Register.Department;
				register.StSkidka = stSkidka;
				register.Skidka = num;
				list.Add(register);
			}
		}
		return list;
	}

	public bool MarkingCodeIsBad(uint ValidationResult)
	{
		if (ValidationResult == 1 || ValidationResult == 17 || ValidationResult == 5 || ValidationResult == 7)
		{
			return true;
		}
		return false;
	}

	public string GetMarkingCodeDecryptionResult(uint Rez2106, uint Rez2105 = 0u, uint Rez2109 = 1u, uint CodeRezultFN = 0u)
	{
		string text = "";
		switch (Rez2106)
		{
		case 0u:
			text = "[М] Проверка КП КМ не выполнена, статус товара ОИСМ не проверен";
			break;
		case 1u:
			text = "[М-] Проверка КП КМ выполнена в ФН с отрицательным результатом, статус товара ОИСМ не проверен";
			break;
		case 3u:
			text = "[М] Проверка КП КМ выполнена с положительным результатом, статус товара ОИСМ не проверен";
			break;
		case 16u:
			text = "[М] Проверка КП КМ не выполнена, статус товара ОИСМ не проверен (ККТ функционирует в автономном режиме)";
			break;
		case 17u:
			text = "[М-] Проверка КП КМ выполнена в ФН с отрицательным результатом, статус товара ОИСМ не проверен (ККТ функционирует в автономном режиме)";
			break;
		case 19u:
			text = "[М] Проверка КП КМ выполнена в ФН с положительным результатом, статус товара ОИСМ не проверен (ККТ функционирует в автономном режиме)";
			break;
		case 5u:
			text = "[М-] Проверка КП КМ выполнена с отрицательным результатом, статус товара у ОИСМ некорректен";
			break;
		case 7u:
			text = "[М-] Проверка КП КМ выполнена с положительным результатом, статус товара у ОИСМ некорректен";
			break;
		case 15u:
			text = "[М+] Проверка КП КМ выполнена с положительным результатом, статус товара у ОИСМ корректен";
			break;
		}
		switch (Rez2105)
		{
		case 1u:
			text += "; Запрос КМ имеет некорректный формат";
			break;
		case 2u:
			text += "; Указанный в запросе КМ имеет некорректный формат(не распознан)";
			break;
		}
		switch (Rez2109)
		{
		case 2u:
			text += "; Планируемый статус товара некорректен";
			break;
		case 3u:
			text += "; Оборот товара приостановлен";
			break;
		}
		if (Rez2106 != 0)
		{
			switch (CodeRezultFN)
			{
			case 1u:
				text += "; КМ данного типа не подлежит проверке в ФН";
				break;
			case 2u:
				text += "; ФН не содержит ключ проверки кода проверки этого КМ";
				break;
			case 3u:
				text += "; Проверка в ФН невозможна, так как отсутствуют теги 91 и/или 92 или их формат неверный";
				break;
			case 4u:
				text += "; Внутренняя ошибка в ФН при проверке этого КМ";
				break;
			default:
				text = text + "; Ошибка проверки в ФН: " + CodeRezultFN;
				break;
			case 0u:
				break;
			}
		}
		return text;
	}

	public int GetStatusMarkingCode(int TypeCheck, uint? MeasureOfQuantity = null)
	{
		int result = 0;
		switch (TypeCheck)
		{
		case 0:
			result = ((MeasureOfQuantity == 0) ? 1 : 2);
			break;
		case 1:
			result = ((MeasureOfQuantity != 0) ? 4 : 3);
			break;
		case 2:
			result = ((MeasureOfQuantity != 0) ? 4 : 3);
			break;
		case 3:
			result = ((MeasureOfQuantity == 0) ? 1 : 2);
			break;
		case 10:
			result = ((MeasureOfQuantity != 0) ? 4 : 3);
			break;
		case 11:
			result = ((MeasureOfQuantity == 0) ? 1 : 2);
			break;
		case 12:
			result = ((MeasureOfQuantity == 0) ? 1 : 2);
			break;
		case 13:
			result = ((MeasureOfQuantity != 0) ? 4 : 3);
			break;
		}
		return result;
	}

	public bool ChekINN(string INN)
	{
		long num = 0L;
		long num2 = 0L;
		if (INN != null && !(INN == ""))
		{
			if (INN.Length == 10)
			{
				try
				{
					num += 2 * int.Parse(INN[0].ToString() ?? "");
					num += 4 * int.Parse(INN[1].ToString() ?? "");
					num += 10 * int.Parse(INN[2].ToString() ?? "");
					num += 3 * int.Parse(INN[3].ToString() ?? "");
					num += 5 * int.Parse(INN[4].ToString() ?? "");
					num += 9 * int.Parse(INN[5].ToString() ?? "");
					num += 4 * int.Parse(INN[6].ToString() ?? "");
					num += 6 * int.Parse(INN[7].ToString() ?? "");
					num += 8 * int.Parse(INN[8].ToString() ?? "");
					num %= 11;
					if ((num % 10).ToString()[0] != INN[9])
					{
						return false;
					}
				}
				catch
				{
					return false;
				}
			}
			else
			{
				if (INN.Length != 12)
				{
					return false;
				}
				try
				{
					num += 7 * int.Parse(INN[0].ToString() ?? "");
					num += 2 * int.Parse(INN[1].ToString() ?? "");
					num += 4 * int.Parse(INN[2].ToString() ?? "");
					num += 10 * int.Parse(INN[3].ToString() ?? "");
					num += 3 * int.Parse(INN[4].ToString() ?? "");
					num += 5 * int.Parse(INN[5].ToString() ?? "");
					num += 9 * int.Parse(INN[6].ToString() ?? "");
					num += 4 * int.Parse(INN[7].ToString() ?? "");
					num += 6 * int.Parse(INN[8].ToString() ?? "");
					num += 8 * int.Parse(INN[9].ToString() ?? "");
					num2 += 3 * int.Parse(INN[0].ToString() ?? "");
					num2 += 7 * int.Parse(INN[1].ToString() ?? "");
					num2 += 2 * int.Parse(INN[2].ToString() ?? "");
					num2 += 4 * int.Parse(INN[3].ToString() ?? "");
					num2 += 10 * int.Parse(INN[4].ToString() ?? "");
					num2 += 3 * int.Parse(INN[5].ToString() ?? "");
					num2 += 5 * int.Parse(INN[6].ToString() ?? "");
					num2 += 9 * int.Parse(INN[7].ToString() ?? "");
					num2 += 4 * int.Parse(INN[8].ToString() ?? "");
					num2 += 6 * int.Parse(INN[9].ToString() ?? "");
					num2 += 8 * int.Parse(INN[10].ToString() ?? "");
					num %= 11;
					char num3 = (num % 10).ToString()[0];
					num2 %= 11;
					char c = (num2 % 10).ToString()[0];
					if (num3 != INN[10] || c != INN[11])
					{
						return false;
					}
				}
				catch
				{
					return false;
				}
			}
		}
		return true;
	}

	public bool IsGoodCode(DataCommand DataCommand)
	{
		bool result = false;
		DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
		foreach (DataCommand.CheckString checkString in checkStrings)
		{
			if (Kkm.FfdVersion >= 4 && checkString.Register != null && checkString.Register.GoodCodeData != null && !string.IsNullOrEmpty(checkString.Register.GoodCodeData.TryBarCode))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public virtual async Task GetBarcode(DataCommand DataCommand, RezultCommandBarCode RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task OpenBarcode(DataCommand DataCommand, RezultCommandBarCode RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task CloseBarcode(DataCommand DataCommand, RezultCommandBarCode RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task PrintDocument(DataCommand DataCommand, RezultCommandKKm RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task OutputOnCustomerDisplay(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task ClearCustomerDisplay(DataCommand DataCommand, RezultCommand RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task OptionsCustomerDisplay(DataCommand DataCommand, RezultCommandCD RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task Calibrate(DataCommand DataCommand, RezultCommandLibra RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task GetWeight(DataCommand DataCommand, RezultCommandLibra RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task CommandPayTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public virtual async Task ProcessCommandPayTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand, int Command)
	{
		RezultCommand.KeySubLicensing = DataCommand.KeySubLicensing;
		await ComDevice.PostCheck(RezultCommand, this);
		DataCommand DataCommandCustomerDisplay = new DataCommand();
		DataCommandCustomerDisplay.NumDevice = int.Parse(UnitParamets["NumDeviceCustomerDisplay"]);
		DataCommandCustomerDisplay.NoError = DataCommandCustomerDisplay.NumDevice == 0;
		DataCommandCustomerDisplay.Command = "OutputOnCustomerDisplay";
		DataCommandCustomerDisplay.TopString = "Оплата счета по банковской карте";
		DataCommandCustomerDisplay.BottomString = "Сумма: " + DataCommand.Amount + " руб.";
		List<Unit> ListSortUnits = Global.UnitManager.Units.Select(delegate(KeyValuePair<int, Unit> u)
		{
			KeyValuePair<int, Unit> keyValuePair = u;
			return keyValuePair.Value;
		}).ToList();
		List<Unit> ListUnits = UnitManager.GetListUnitsForCommand(DataCommandCustomerDisplay, TypeDevice.enType.ДисплеиПокупателя, ref ListSortUnits);
		if (ListUnits.Count > 0)
		{
			new Task(async delegate
			{
				string textCommand = JsonConvert.SerializeObject(DataCommandCustomerDisplay, new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
				try
				{
					await Global.UnitManager.AddCommand(DataCommandCustomerDisplay, "", textCommand);
				}
				catch (Exception)
				{
				}
			}).Start();
		}
		await CommandPayTerminal(DataCommand, RezultCommand, Command);
		if (ListUnits.Count > 0)
		{
			new Task(async delegate
			{
				DataCommandCustomerDisplay = new DataCommand();
				DataCommandCustomerDisplay.NumDevice = int.Parse(UnitParamets["NumDeviceCustomerDisplay"]);
				DataCommandCustomerDisplay.NoError = DataCommandCustomerDisplay.NumDevice == 0;
				DataCommandCustomerDisplay.Command = "ClearCustomerDisplay";
				string textCommand = JsonConvert.SerializeObject(DataCommandCustomerDisplay, new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
				try
				{
					await Global.UnitManager.AddCommand(DataCommandCustomerDisplay, "", textCommand, "", DataCommand.IdCommand);
				}
				catch (Exception)
				{
				}
			}).Start();
		}
		await PrintSlip(DataCommand, RezultCommand.Slip, IsCheck: true);
	}

	public virtual async Task EmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public async Task ProcessEmergencyReversal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		DataCommand DataCommandCustomerDisplay = new DataCommand();
		DataCommandCustomerDisplay.NumDevice = int.Parse(UnitParamets["NumDeviceCustomerDisplay"]);
		DataCommandCustomerDisplay.NoError = DataCommandCustomerDisplay.NumDevice == 0;
		DataCommandCustomerDisplay.Command = "OutputOnCustomerDisplay";
		DataCommandCustomerDisplay.TopString = "Аварийная отмена оплаты..";
		DataCommandCustomerDisplay.BottomString = "Сумма: " + DataCommand.Amount + " руб.";
		List<Unit> ListSortUnits = Global.UnitManager.Units.Select(delegate(KeyValuePair<int, Unit> u)
		{
			KeyValuePair<int, Unit> keyValuePair = u;
			return keyValuePair.Value;
		}).ToList();
		List<Unit> ListUnits = UnitManager.GetListUnitsForCommand(DataCommandCustomerDisplay, TypeDevice.enType.ДисплеиПокупателя, ref ListSortUnits);
		if (ListUnits.Count > 0)
		{
			new Task(async delegate
			{
				string textCommand = JsonConvert.SerializeObject(DataCommandCustomerDisplay, new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
				try
				{
					await Global.UnitManager.AddCommand(DataCommandCustomerDisplay, "", textCommand);
				}
				catch (Exception)
				{
				}
			}).Start();
		}
		await EmergencyReversal(DataCommand, RezultCommand);
		if (ListUnits.Count > 0)
		{
			new Task(async delegate
			{
				DataCommandCustomerDisplay = new DataCommand();
				DataCommandCustomerDisplay.NumDevice = int.Parse(UnitParamets["NumDeviceCustomerDisplay"]);
				DataCommandCustomerDisplay.NoError = DataCommandCustomerDisplay.NumDevice == 0;
				DataCommandCustomerDisplay.Command = "ClearCustomerDisplay";
				string textCommand = JsonConvert.SerializeObject(DataCommandCustomerDisplay, new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
				try
				{
					await Global.UnitManager.AddCommand(DataCommandCustomerDisplay, "", textCommand, "", DataCommand.IdCommand);
				}
				catch (Exception)
				{
				}
			}).Start();
		}
		await PrintSlip(DataCommand, RezultCommand.Slip, IsCheck: true);
	}

	public virtual async Task Settlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public async Task ProcessSettlement(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		if (DataCommand.Timeout < 300)
		{
			DataCommand.Timeout = 300;
		}
		await Settlement(DataCommand, RezultCommand);
		await PrintSlip(DataCommand, RezultCommand.Slip, IsCheck: false);
	}

	public virtual async Task TerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Error;
		RezultCommand.Error = "Драйвер не поддерживает эту команду";
	}

	public async Task ProcessTerminalReport(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		await TerminalReport(DataCommand, RezultCommand);
		await PrintSlip(DataCommand, RezultCommand.Slip, IsCheck: false);
	}

	public virtual async Task TransactionDetails(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Error;
		RezultCommand.Error = "Драйвер не поддерживает эту команду";
	}

	public async Task ProcessTransactionDetails(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		await TransactionDetails(DataCommand, RezultCommand);
		await PrintSlip(DataCommand, RezultCommand.Slip, IsCheck: false);
	}

	public virtual async Task PrintSlipOnTerminal(DataCommand DataCommand, RezultCommandProcessing RezultCommand)
	{
		RezultCommand.Status = ExecuteStatus.Ok;
	}

	public async Task PrintSlip(DataCommand DataCommand, string Slip, bool IsCheck)
	{
		if (SettDr.Paramets["NumDeviceByPrintSlip"] != "" && DataCommand.NotPrint == false && Slip != null && Slip != "")
		{
			new Task(async delegate
			{
				bool PrintSlip = SettDr.Paramets["PrintSlip"].AsBool();
				bool PrintSlipForCashier = SettDr.Paramets["PrintSlipForCashier"].AsBool();
				if (!IsCheck)
				{
					PrintSlipForCashier = false;
				}
				int NumDeviceByPrintSlip = int.Parse(SettDr.Paramets["NumDeviceByPrintSlip"]);
				if (NumDeviceByPrintSlip == 0)
				{
					foreach (KeyValuePair<int, Global.DeviceSettings> device in Global.Settings.Devices)
					{
						if ((device.Value.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор && Kkm.INN == "") || device.Value.INN == Kkm.INN)
						{
							NumDeviceByPrintSlip = device.Value.NumDevice;
							break;
						}
					}
				}
				if (Global.UnitManager.Units[NumDeviceByPrintSlip].Kkm.PrintingWidth == 0)
				{
					DataCommand dataCommand = new DataCommand
					{
						Command = "GetDataKKT",
						NumDevice = NumDeviceByPrintSlip,
						RunComPort = true
					};
					NetLogs.Append("\r\n\r\nТестируем ККТ GetDataKKT");
					await Global.UnitManager.AddCommand(dataCommand, "", "");
				}
				if (PrintSlip)
				{
					DataCommand dataCommand2 = await CreateCommandSlip(Slip, Global.UnitManager.Units[NumDeviceByPrintSlip].Kkm.PrintingWidth);
					dataCommand2.NumDevice = NumDeviceByPrintSlip;
					dataCommand2.RunComPort = false;
					string textCommand = JsonConvert.SerializeObject(dataCommand2, new JsonSerializerSettings
					{
						StringEscapeHandling = StringEscapeHandling.EscapeHtml
					});
					NetLogs.Append("\r\n\r\nПечатаем слип эквайринга для клиента");
					await Global.UnitManager.AddCommand(dataCommand2, "", textCommand);
				}
				if (PrintSlipForCashier && DataCommand.Command != "TerminalReport" && DataCommand.Command != "Settlement")
				{
					DataCommand dataCommand3 = await CreateCommandSlip(Slip, Global.UnitManager.Units[NumDeviceByPrintSlip].Kkm.PrintingWidth);
					dataCommand3.NumDevice = NumDeviceByPrintSlip;
					dataCommand3.RunComPort = false;
					string textCommand2 = JsonConvert.SerializeObject(dataCommand3, new JsonSerializerSettings
					{
						StringEscapeHandling = StringEscapeHandling.EscapeHtml
					});
					NetLogs.Append("\r\n\r\nПечатаем слип эквайринга для кассира");
					await Global.UnitManager.AddCommand(dataCommand3, "", textCommand2);
				}
			}).Start();
		}
		if (SettDr.Paramets["PrintSlipOnWindows"].AsBool() && Slip != null && Slip != "")
		{
			int num = 48;
			string[] array = Slip.Replace("\r", "").Split('\n');
			int num2 = 0;
			string[] array2 = array;
			foreach (string text in array2)
			{
				num2 = Math.Max(num2, text.Length);
			}
			string text2 = ((num - num2 <= 0) ? "" : "".PadLeft((num - num2) / 2));
			for (int num4 = 0; num4 < array.Length; num4++)
			{
				string text3 = array[num4].Replace("\n", "");
				WriteLine(text2 + text3, 1, Clear: false, null, AsCheck: true);
			}
			WriteLine("", 0, Clear: false, true, AsCheck: true);
		}
	}

	public virtual void GetRezult(DataCommand DataCommand, RezultCommand RezultCommand)
	{
	}

	public virtual async Task<bool> PortOpenAsync()
	{
		return true;
	}

	public virtual async Task<bool> PortOpenAsync(Parity Parity = Parity.None)
	{
		return true;
	}

	public virtual async Task<bool> PortCloseAsync()
	{
		return true;
	}

	public string GetStringFromDict(RezultCommandProcessing RezultCommand = null, string Id1 = null, string Val1 = null, string Id2 = null, string Val2 = null, string Id3 = null, string Val3 = null, string Id4 = null, string Val4 = null)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 1; i <= 11; i++)
		{
			string text = "";
			string text2 = "";
			switch (i)
			{
			case 1:
				text = "CN";
				text2 = RezultCommand.CardNumber;
				break;
			case 2:
				text = "RN";
				text2 = RezultCommand.ReceiptNumber;
				break;
			case 3:
				text = "RRN";
				text2 = RezultCommand.RRNCode;
				break;
			case 4:
				text = "AC";
				text2 = RezultCommand.AuthorizationCode;
				break;
			case 5:
				text = "CH";
				text2 = RezultCommand.CardHash;
				break;
			case 6:
				text = "CD";
				text2 = RezultCommand.CardDPAN;
				break;
			case 7:
				text = "ID";
				text2 = RezultCommand.IdProcessing;
				break;
			case 8:
				text = "ED";
				text2 = RezultCommand.CardEncryptedData;
				break;
			case 9:
				text = Id1;
				text2 = Val1;
				break;
			case 10:
				text = Id2;
				text2 = Val2;
				break;
			case 11:
				text = Id3;
				text2 = Val3;
				break;
			case 12:
				text = Id4;
				text2 = Val4;
				break;
			}
			if (text != null && text2 != null && !(text.Trim() == "") && !(text2.Trim() == ""))
			{
				text2 = text2.Replace(":", "\\.");
				text2 = text2.Replace(";", "\\,");
				text2 = text2.Replace("\\", "\\\\");
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(";");
				}
				stringBuilder.Append(text);
				stringBuilder.Append(":");
				stringBuilder.Append(text2);
			}
		}
		return stringBuilder.ToString();
	}

	public Dictionary<string, object> SetDictFromString(string TransactionID, DataCommand DataCommand)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (TransactionID == null)
		{
			TransactionID = "";
		}
		string[] array = TransactionID.Split(';');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[1] { ':' }, StringSplitOptions.None);
			if (array2.Length < 2)
			{
				continue;
			}
			string text = array2[1];
			if (array2[0] != null && text != null && !(array2[0].Trim() == "") && !(text.Trim() == ""))
			{
				text = text.Replace("\\.", ":");
				text = text.Replace("\\,", ";");
				text = text.Replace("\\\\", "\\");
				switch (array2[0])
				{
				case "CN":
					DataCommand.CardNumber = text;
					break;
				case "RN":
					DataCommand.ReceiptNumber = text;
					break;
				case "RRN":
					DataCommand.RRNCode = text;
					break;
				case "AC":
					DataCommand.AuthorizationCode = text;
					break;
				case "ID":
					DataCommand.IdProcessing = text;
					break;
				default:
					dictionary.Add(array2[0], text);
					break;
				}
			}
		}
		return dictionary;
	}

	public void WriteLine(object PrnObject, int Font = 0, bool Clear = false, bool? RunStop = null, bool AsCheck = false)
	{
		Global.WriteLine(PrnObject, Font = 0, Clear, RunStop, AsCheck, this);
	}

	public void UpdateSettingsServer()
	{
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор)
		{
			if (Kkm.NumberKkm != null && Kkm.NumberKkm != "" && SettDr.NumberKkm != Kkm.NumberKkm)
			{
				SettDr.NumberKkm = Kkm.NumberKkm;
				Global.SettingsServerModified = true;
			}
			if (Kkm.INN != null && Kkm.INN != "" && SettDr.INN != Kkm.INN)
			{
				SettDr.INN = Kkm.INN;
				Global.SettingsServerModified = true;
			}
			if (Kkm.TaxVariant != null && Kkm.TaxVariant != "" && SettDr.TaxVariant != Kkm.TaxVariant)
			{
				SettDr.TaxVariant = Kkm.TaxVariant;
				Global.SettingsServerModified = true;
			}
		}
	}

	public void LoadParametsFromXML(string ParamsXML)
	{
		if (Global.Settings.Marke != "" && Global.Settings.Marke != "KkmServer")
		{
			while (true)
			{
				int num = ParamsXML.ToLower().IndexOf("https://kkmserver.ru/WiKi".ToLower());
				if (num == -1)
				{
					break;
				}
				int num2;
				for (num2 = num; num2 != 0; num2--)
				{
					string text = ParamsXML.Substring(num2 - 1, 1);
					if (text == "\r" || text == "\n")
					{
						break;
					}
				}
				int i;
				for (i = num; i != ParamsXML.Length - 1; i++)
				{
					string text2 = ParamsXML.Substring(i + 1, 1);
					if (text2 == "\r" || text2 == "\n")
					{
						break;
					}
				}
				ParamsXML = ParamsXML.Substring(0, num2) + ParamsXML.Substring(i + 1);
			}
		}
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(ParamsXML);
		XmlNode documentElement = xmlDocument.DocumentElement;
		LoadParametsItems((XmlElement)documentElement, "", "");
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор || SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
		{
			UnitParamets.Add("Save_INN", "");
			if (!SettDr.Paramets.ContainsKey("Save_INN"))
			{
				SettDr.Paramets.Add("Save_INN", "");
			}
			else
			{
				Kkm.INN = SettDr.Paramets["Save_INN"];
				UnitParamets["Save_INN"] = Kkm.INN;
			}
			UnitParamets.Add("Save_NumberKkm", "");
			if (!SettDr.Paramets.ContainsKey("Save_NumberKkm"))
			{
				SettDr.Paramets.Add("Save_NumberKkm", "");
			}
			else
			{
				Kkm.NumberKkm = SettDr.Paramets["Save_NumberKkm"];
				UnitParamets["Save_NumberKkm"] = Kkm.NumberKkm;
			}
			UnitParamets.Add("Save_TaxVariant", "");
			if (!SettDr.Paramets.ContainsKey("Save_TaxVariant"))
			{
				SettDr.Paramets.Add("Save_TaxVariant", "");
			}
			else
			{
				Kkm.TaxVariant = SettDr.Paramets["Save_TaxVariant"];
				UnitParamets["Save_TaxVariant"] = Kkm.TaxVariant;
			}
		}
		if (SettDr.TypeDevice.Id == "KkmAtol" || SettDr.TypeDevice.Id == "KkmAtol")
		{
			UnitParamets.Add("KktOfdNet", "AtolKKT");
			if (!SettDr.Paramets.ContainsKey("KktOfdNet"))
			{
				SettDr.Paramets.Add("KktOfdNet", "");
			}
			iUnitSettings.Paramert paramert = new iUnitSettings.Paramert();
			paramert.Page = "Параметры";
			paramert.Group = "Настройка связи с ОФД";
			paramert.Caption = "COM порт для связи с ОФД";
			paramert.Name = "KktOfdNet";
			paramert.TypeValue = "String";
			paramert.Help = "Настройка сервиса PPP Ethernet Over Usb.\r\n                                Эта настройка определяет как ККТ будет передавать данные в ОФД.</br>\r\n                                Если ККТ настроен на самостоятельный обмен через Ethernet или если обмен идет через ДТО то выбирайте Ehternet.</br>\r\n                                Если выбран COM порт то будет установлен и запущен сервис передачи данных PPP Ethernet Over Usb.";
			paramert.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Ethernet/WiFi (ККТ самостоятельно обменивается с ОФД)",
				Value = ""
			});
			try
			{
				foreach (KeyValuePair<string, string> item in ((UnitPort)this).GetListComPort())
				{
					paramert.VauePars.Add(new iUnitSettings.VauePar
					{
						Caption = item.Value + " (EthernetOverUsb сервис будет запущен)",
						Value = item.Key
					});
				}
			}
			catch
			{
			}
			if (GetType() == typeof(StrihM))
			{
				paramert.Description = "Если ККТ 'Штрих-М' находится в режиме интерфейса 'Ethernet/WiFi' то для передачи в ОФД используются сетевые порты.\r\n                                        Соответственно сеть к которой подключен ККТ должна иметь выход в Интернет.\r\n                                        \r\n                                        Если ККТ находится в режиме интерфейса 'USB(протокол RNDIS)' то сетевой интерфейс в ПК должен быть настроен на передачу данных в интернет.\r\n\r\n                                        Если ККТ находится в режиме интерфейса 'USB-to-COM' или 'COM' то для передачи данных в ОФД должна использоватся технология 'PPP Ethernet Over Usb.'\r\n                                        Для ее активации:\r\n                                        1. ККТ должен быть подключен к ПК через USB.\r\n                                        2. ККТ должен быть одновременно подключен к ПК через СОМ порт.\r\n                                        По одному порту надо передавать команды ККТ, по другому - PPP Ethernet Over Usb\r\n                                        \r\n                                        Инструкция по программированию различных интерфейсов:\r\n                                        <a id=\"help\" href=\"https://kkmserver.ru/Donload/Shtrih-M_settings_net_for_OFD.pdf\">https://kkmserver.ru/Donload/Shtrih-M_settings_net_for_OFD.pdf</a> \r\n\r\n                                        Настоятельно рекомендуется подключать ККТ через Ehternet порт или RNDIS-Ehternet.\r\n                                        Проводов меньше, связь с ОФД более устойчива, настройка проще.\r\n                                        ".Replace("\n", "\n<br/>");
			}
			UnitSettings.Paramerts.Add(paramert);
		}
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор || SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
		{
			UnitParamets.Add("KktChangeSCO_31_32", ExtensionMethods.AsString(Val: true));
			if (!SettDr.Paramets.ContainsKey("KktChangeSCO_31_32"))
			{
				SettDr.Paramets.Add("KktChangeSCO_31_32", "false");
			}
			iUnitSettings.Paramert paramert2 = new iUnitSettings.Paramert();
			paramert2.Page = "Параметры";
			paramert2.Group = "Общие параметры";
			paramert2.Caption = "Замена Sig.Calc.Object 1/2 на 33/31";
			paramert2.Name = "KktChangeSCO_31_32";
			paramert2.TypeValue = "String";
			paramert2.Description = "При регистрации маркированного товара нужно передавать SignCalculationObject 33/31 вместо 1/2.</br>\r\n                                        Но не все ККТ это поддерживают";
			paramert2.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Не заменять",
				Value = "false"
			});
			paramert2.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Заменять 1/2 на 33/31",
				Value = "true"
			});
			paramert2.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Заменять 33/31 на 1/2",
				Value = "back"
			});
			UnitSettings.Paramerts.Add(paramert2);
		}
		if (UseBuiltTerminal && (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор || (SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков && SettDr.Paramets.ContainsKey("EmulationCheck") && SettDr.Paramets["EmulationCheck"].AsBool())))
		{
			UnitParamets.Add("UseBuiltTerminal", ExtensionMethods.AsString(Val: true));
			if (!SettDr.Paramets.ContainsKey("UseBuiltTerminal"))
			{
				SettDr.Paramets.Add("UseBuiltTerminal", ExtensionMethods.AsString(Val: true));
			}
			if (SettDr.Paramets["UseBuiltTerminal"].GetType() == typeof(string))
			{
				SettDr.Paramets["UseBuiltTerminal"] = ExtensionMethods.AsString(Val: true);
			}
			iUnitSettings.Paramert paramert3 = new iUnitSettings.Paramert();
			paramert3.Page = "Параметры";
			paramert3.Group = "Автоматический Эквайринг (от чеков)";
			paramert3.Caption = "Встроенный В ККТ эквайринг";
			paramert3.Name = "UseBuiltTerminal";
			paramert3.TypeValue = "Boolean";
			paramert3.Description = "При регистрации чека с \"электронной\" оплатой предварительно проводить оплату через встроенный эквайринговый терминал.</br>\r\n                                        Если установить 'Не использовать' то чек с \"электронной\" оплатой печатается сразу - без проведения оплаты по карте";
			paramert3.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Не использовать",
				Value = "False"
			});
			paramert3.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Использовать",
				Value = "True"
			});
			UnitSettings.Paramerts.Add(paramert3);
		}
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор || (SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков && SettDr.Paramets.ContainsKey("EmulationCheck") && SettDr.Paramets["EmulationCheck"].AsBool()))
		{
			UnitParamets.Add("PayByProcessing", "");
			if (!SettDr.Paramets.ContainsKey("PayByProcessing"))
			{
				SettDr.Paramets.Add("PayByProcessing", "");
			}
			iUnitSettings.Paramert paramert4 = new iUnitSettings.Paramert();
			paramert4.Page = "Параметры";
			paramert4.Group = "Автоматический Эквайринг (от чеков)";
			paramert4.Caption = "Использовать эквайринг";
			paramert4.Name = "PayByProcessing";
			paramert4.TypeValue = "String";
			if (SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
			{
				paramert4.MasterParameterName = "EmulationCheck";
				paramert4.MasterParameterOperation = "Equal";
				paramert4.MasterParameterValue = "True";
			}
			paramert4.Description = "При регистрации чека с \"электронной\" оплатой предварительно проводить оплату через эквайринговый терминал.";
			paramert4.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Без эквайринга",
				Value = ""
			});
			paramert4.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Слип-чек в чеке. Без дополнительного Слип-чека для кассира",
				Value = "1"
			});
			paramert4.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Слип-чек отдельно. Без дополнительного Слип-чека для кассира",
				Value = "2"
			});
			paramert4.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Слип-чек в чеке. Дополнительный Слип-чек для кассира",
				Value = "3"
			});
			paramert4.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Слип-чек отдельно. Дополнительный Слип-чек для кассира",
				Value = "4"
			});
			UnitSettings.Paramerts.Add(paramert4);
			UnitParamets.Add("NumDeviceByProcessing", "");
			if (!SettDr.Paramets.ContainsKey("NumDeviceByProcessing"))
			{
				SettDr.Paramets.Add("NumDeviceByProcessing", "0");
			}
			paramert4 = new iUnitSettings.Paramert();
			paramert4.Page = "Параметры";
			paramert4.Group = "Автоматический Эквайринг (от чеков)";
			paramert4.Caption = "Устройство для эквайринга";
			paramert4.Name = "NumDeviceByProcessing";
			paramert4.TypeValue = "String";
			if (SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
			{
				paramert4.MasterParameterName = "EmulationCheck";
				paramert4.MasterParameterOperation = "Equal";
				paramert4.MasterParameterValue = "True";
			}
			paramert4.Description = "При регистрации чека с \"электронной\" оплатой предварительно проводить оплату через указанный эквайринговый терминал.";
			paramert4.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Любое устройство эквайринга",
				Value = "0"
			});
			foreach (KeyValuePair<int, Global.DeviceSettings> device in Global.Settings.Devices)
			{
				if (device.Value.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал)
				{
					paramert4.VauePars.Add(new iUnitSettings.VauePar
					{
						Caption = device.Value.NumDevice + " - " + device.Value.TypeDevice.Protocol,
						Value = device.Value.NumDevice.ToString()
					});
				}
			}
			UnitSettings.Paramerts.Add(paramert4);
			UnitParamets.Add("SettlementInCloseShift", ExtensionMethods.AsString(Val: true));
			if (!SettDr.Paramets.ContainsKey("SettlementInCloseShift"))
			{
				SettDr.Paramets.Add("SettlementInCloseShift", ExtensionMethods.AsString(Val: true));
			}
			paramert4 = new iUnitSettings.Paramert();
			paramert4.Page = "Параметры";
			paramert4.Group = "Автоматический Эквайринг (от чеков)";
			paramert4.Caption = "'Итоги дня' при закрытии смены";
			paramert4.Name = "SettlementInCloseShift";
			paramert4.TypeValue = "Boolean";
			if (SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
			{
				paramert4.MasterParameterName = "EmulationCheck";
				paramert4.MasterParameterOperation = "Equal";
				paramert4.MasterParameterValue = "True";
			}
			paramert4.Description = "При закрытии смены на ККТ выполнять на эквайринговом терминале Сверку итогов дня.";
			UnitSettings.Paramerts.Add(paramert4);
		}
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал)
		{
			UnitParamets.Add("NumDeviceByPrintSlip", "");
			if (!SettDr.Paramets.ContainsKey("NumDeviceByPrintSlip"))
			{
				SettDr.Paramets.Add("NumDeviceByPrintSlip", "0");
			}
			iUnitSettings.Paramert paramert5 = new iUnitSettings.Paramert();
			paramert5.Page = "Параметры";
			paramert5.Group = "Печать слип-чека";
			paramert5.Caption = "Устройство для печати";
			paramert5.Name = "NumDeviceByPrintSlip";
			paramert5.TypeValue = "String";
			paramert5.Description = "Печать слип-чек на выбранном устройстве";
			paramert5.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Не печатать автоматически",
				Value = ""
			});
			paramert5.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Любое ККТ",
				Value = "0"
			});
			foreach (KeyValuePair<int, Global.DeviceSettings> device2 in Global.Settings.Devices)
			{
				if (device2.Value.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор || device2.Value.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
				{
					paramert5.VauePars.Add(new iUnitSettings.VauePar
					{
						Caption = device2.Value.NumDevice + " - " + device2.Value.TypeDevice.Protocol,
						Value = device2.Value.NumDevice.ToString()
					});
				}
			}
			UnitSettings.Paramerts.Add(paramert5);
			UnitParamets.Add("PrintSlip", ExtensionMethods.AsString(Val: true));
			if (!SettDr.Paramets.ContainsKey("PrintSlip"))
			{
				SettDr.Paramets.Add("PrintSlip", ExtensionMethods.AsString(Val: true));
			}
			paramert5 = new iUnitSettings.Paramert();
			paramert5.Page = "Параметры";
			paramert5.Group = "Печать слип-чека";
			paramert5.Caption = "Печатать слип-чек";
			paramert5.Name = "PrintSlip";
			paramert5.TypeValue = "Boolean";
			paramert5.Description = "Печатать слип-чек для клиента.";
			UnitSettings.Paramerts.Add(paramert5);
			UnitParamets.Add("PrintSlipForCashier", ExtensionMethods.AsString(Val: true));
			if (!SettDr.Paramets.ContainsKey("PrintSlipForCashier"))
			{
				SettDr.Paramets.Add("PrintSlipForCashier", ExtensionMethods.AsString(Val: true));
			}
			paramert5 = new iUnitSettings.Paramert();
			paramert5.Page = "Параметры";
			paramert5.Group = "Печать слип-чека";
			paramert5.Caption = "Печатать слип-чек для кассира";
			paramert5.Name = "PrintSlipForCashier";
			paramert5.TypeValue = "Boolean";
			paramert5.Description = "Печатать дополнительный слип-чек для кассира.";
			UnitSettings.Paramerts.Add(paramert5);
			UnitParamets.Add("PrintSlipOnWindows", ExtensionMethods.AsString(Val: true));
			if (!SettDr.Paramets.ContainsKey("PrintSlipOnWindows"))
			{
				if (SettDr.TypeDevice.UnitDevice == TypeDevice.enUnitDevice.GateEmulator)
				{
					SettDr.Paramets.Add("PrintSlipOnWindows", ExtensionMethods.AsString(Val: true));
				}
				else
				{
					SettDr.Paramets.Add("PrintSlipOnWindows", ExtensionMethods.AsString(Val: false));
				}
			}
			paramert5 = new iUnitSettings.Paramert();
			paramert5.Page = "Параметры";
			paramert5.Group = "Печать слип-чека";
			paramert5.Caption = "Показывать слип-чек на экране";
			paramert5.Name = "PrintSlipOnWindows";
			paramert5.TypeValue = "Boolean";
			paramert5.Description = "Показывать слип-чек на экране после проведения транзакции";
			UnitSettings.Paramerts.Add(paramert5);
		}
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал)
		{
			UnitParamets.Add("INN", "");
			if (!SettDr.Paramets.ContainsKey("INN"))
			{
				SettDr.Paramets.Add("INN", "");
			}
			iUnitSettings.Paramert paramert6 = new iUnitSettings.Paramert();
			paramert6.Page = "Параметры";
			paramert6.Group = "Печать слип-чека";
			paramert6.Caption = "INN организации эквайринга";
			paramert6.Name = "INN";
			paramert6.TypeValue = "String";
			paramert6.Description = "Для поиска нужного терминала. <br/>Если не указанно то ИНН будет брать из ККТ, назначенное на печать";
			UnitSettings.Paramerts.Add(paramert6);
		}
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал)
		{
			UnitParamets.Add("NumDeviceCustomerDisplay", "");
			if (!SettDr.Paramets.ContainsKey("NumDeviceCustomerDisplay"))
			{
				SettDr.Paramets.Add("NumDeviceCustomerDisplay", "0");
			}
			iUnitSettings.Paramert paramert7 = new iUnitSettings.Paramert();
			paramert7 = new iUnitSettings.Paramert();
			paramert7.Page = "Параметры";
			paramert7.Group = "Дисплей покупателя";
			paramert7.Caption = "Дисплей для вывода сообщений";
			paramert7.Name = "NumDeviceCustomerDisplay";
			paramert7.TypeValue = "String";
			paramert7.Description = "Выводить сообщения через указанный Дисплей покупателя.";
			paramert7.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Любой дисплей покупателя",
				Value = "0"
			});
			foreach (KeyValuePair<int, Global.DeviceSettings> device3 in Global.Settings.Devices)
			{
				if (device3.Value.TypeDevice.Type == TypeDevice.enType.ДисплеиПокупателя)
				{
					paramert7.VauePars.Add(new iUnitSettings.VauePar
					{
						Caption = device3.Value.NumDevice + " - " + device3.Value.TypeDevice.Protocol,
						Value = device3.Value.NumDevice.ToString()
					});
				}
			}
			UnitSettings.Paramerts.Add(paramert7);
		}
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор)
		{
			UnitParamets.Add("PrintOnPage", "");
			if (!SettDr.Paramets.ContainsKey("PrintOnPage"))
			{
				SettDr.Paramets.Add("PrintOnPage", "");
			}
			iUnitSettings.Paramert paramert8 = new iUnitSettings.Paramert();
			paramert8.Page = "Параметры";
			paramert8.Group = "Дополнительные настройки";
			paramert8.Caption = "Печать чека и отчета на ленте";
			paramert8.Name = "PrintOnPage";
			paramert8.TypeValue = "String";
			paramert8.Description = "Настройка печати/не печати текста на чековой ленте.";
			paramert8.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Из команды. Если там нет - Печатать.",
				Value = ""
			});
			paramert8.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Из команды. Если там нет - Не печатать.",
				Value = "NotPrint"
			});
			paramert8.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Из команды. Если там нет - Не печатать. Но печатать отчеты.",
				Value = "NotPrint+Report"
			});
			paramert8.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Всегда печатать. Признак из команды игнорировать",
				Value = "AlwaysPrint"
			});
			paramert8.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Всегда Не печатать. Признак из команды игнорировать",
				Value = "AlwaysNotPrint"
			});
			paramert8.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Всегда Не печатать. Но печатать отчеты. Признак из команды игнорировать",
				Value = "AlwaysNotPrint+Report"
			});
			UnitSettings.Paramerts.Add(paramert8);
		}
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор)
		{
			UnitParamets.Add("PaymentCashOnClouseShift", "");
			if (!SettDr.Paramets.ContainsKey("PaymentCashOnClouseShift"))
			{
				SettDr.Paramets.Add("PaymentCashOnClouseShift", "");
			}
			iUnitSettings.Paramert paramert9 = new iUnitSettings.Paramert();
			paramert9.Page = "Параметры";
			paramert9.Group = "Дополнительные настройки";
			paramert9.Caption = "Инкассация при закрытии смены";
			paramert9.Name = "PaymentCashOnClouseShift";
			paramert9.TypeValue = "String";
			paramert9.Description = "При закрытии смены регистрировать чек на инкассацию всех наличных денежных средств в кассе.";
			paramert9.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Ничего не делать",
				Value = ""
			});
			paramert9.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Инкасация в отчете о закрытии (не все ККТ)",
				Value = "Zreport"
			});
			paramert9.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Делать инкассацию отдельным чеком",
				Value = "True"
			});
			UnitSettings.Paramerts.Add(paramert9);
		}
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор)
		{
			UnitParamets.Add("SetDateTime", ExtensionMethods.AsString(Val: false));
			if (!SettDr.Paramets.ContainsKey("SetDateTime"))
			{
				SettDr.Paramets.Add("SetDateTime", ExtensionMethods.AsString(Val: false));
			}
			iUnitSettings.Paramert paramert10 = new iUnitSettings.Paramert();
			paramert10.Page = "Параметры";
			paramert10.Group = "Дополнительные настройки";
			paramert10.Caption = "Установка даты и времени ККТ при закрытии смены";
			paramert10.Name = "SetDateTime";
			paramert10.TypeValue = "Boolean";
			paramert10.Description = "При закрытии смены дата и время будут установлены в ККТ по времени ПК.</br>\r\n                                        Если на ККТ используется время другой временной зоны - выключите эту функцию.";
			UnitSettings.Paramerts.Add(paramert10);
		}
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор)
		{
			UnitParamets.Add("NoErrorOnOpenCloseShift", ExtensionMethods.AsString(Val: false));
			if (!SettDr.Paramets.ContainsKey("NoErrorOnOpenCloseShift"))
			{
				SettDr.Paramets.Add("NoErrorOnOpenCloseShift", ExtensionMethods.AsString(Val: false));
			}
			iUnitSettings.Paramert paramert11 = new iUnitSettings.Paramert();
			paramert11.Page = "Параметры";
			paramert11.Group = "Дополнительные настройки";
			paramert11.Caption = "Сброс ошибки при открытии/закрытии смены";
			paramert11.Name = "NoErrorOnOpenCloseShift";
			paramert11.TypeValue = "Boolean";
			paramert11.Description = "При открытии/закрытии смены будет сбрасываться ошибки (если смена уже была открыта или закрыта).</br>\r\n                                        </br>\r\n                                        Это нужно когда идет параллельная работа с ККТ из нескольких источников и с 1с.</br>\r\n                                        1с запоминает была-ли смена отрыта/закрыта через нее и если это не так не дает работать.</br>\r\n                                        Именно для обхода этой ошибки в 1с и предназначена эта опция";
			UnitSettings.Paramerts.Add(paramert11);
		}
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор || SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
		{
			UnitParamets.Add("EmulationCheck", ExtensionMethods.AsString(Val: false));
			if (!SettDr.Paramets.ContainsKey("EmulationCheck"))
			{
				SettDr.Paramets.Add("EmulationCheck", ExtensionMethods.AsString(Val: false));
			}
			UnitParamets.Add("RouteCommand", "");
			if (!SettDr.Paramets.ContainsKey("RouteCommand"))
			{
				SettDr.Paramets.Add("RouteCommand", "");
			}
			UnitParamets.Add("EmulationCheckForm", GetEmulationCheckForm());
			if (!SettDr.Paramets.ContainsKey("EmulationCheckForm"))
			{
				SettDr.Paramets.Add("EmulationCheckForm", GetEmulationCheckForm());
			}
			iUnitSettings.Paramert paramert12 = new iUnitSettings.Paramert();
			paramert12.Page = "Параметры";
			paramert12.Group = "Эмуляция чека / маршрутизация команд";
			if (SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
			{
				paramert12.Caption = "Печать не-фискального чека";
			}
			else
			{
				paramert12.Caption = "Фискальный/Не-фискальный чек";
			}
			paramert12.Name = "EmulationCheck";
			paramert12.TypeValue = "Boolean";
			paramert12.Description = "Печать не-фискального чека по данным фискального чека.</br>\r\n                    Только для некоторых ИП которым разрешено законом не печатать фискальный чек.</br>\r\n                    </br>\r\n                    Данные не будут передаватся в налоговую.</br>\r\n                    Возможно использовать ККТ без ФН (для большинства моделей ККТ, но не все позволяют)";
			if (SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
			{
				paramert12.VauePars.Add(new iUnitSettings.VauePar
				{
					Caption = "Отключено",
					Value = "False"
				});
			}
			else
			{
				paramert12.VauePars.Add(new iUnitSettings.VauePar
				{
					Caption = "Печать фискального чека",
					Value = "False"
				});
			}
			paramert12.VauePars.Add(new iUnitSettings.VauePar
			{
				Caption = "Печать НЕ-фискального чека (Эмуляция)",
				Value = "True"
			});
			UnitSettings.Paramerts.Add(paramert12);
			paramert12 = new iUnitSettings.Paramert();
			paramert12.Page = "Параметры";
			paramert12.Group = "Эмуляция чека / маршрутизация команд";
			paramert12.Caption = "Маршрутизация команд";
			paramert12.Name = "RouteCommand";
			paramert12.TypeValue = "String";
			paramert12.MasterParameterName = "EmulationCheck";
			paramert12.MasterParameterOperation = "Equal";
			paramert12.MasterParameterValue = "True";
			paramert12.Help = "Пример заполнения: http&lt;s&gt;://&lt;Login&gt;:&lt;Password&gt;@&lt;Url&gt;:&lt;Port&gt;/&lt;NumDevice&gt;";
			paramert12.Description = "<br/><br/>\r\n                        Маршрутизация выполнения команды на другой kkmserver.<br/>\r\n                        <br/>\r\n                        Пример заполнение:<br/>\r\n                        https://Login:Password@org.ru:5893/1<br/>\r\n                        <br/>\r\n                        Где:<br/>\r\n                        'Login' - Имя пользователя на уддаленном kkmserver<br/>\r\n                        'Password' - Пароль пользователя на уддаленном kkmserver<br/>\r\n                        'org.ru' - путь к удаленному kkmserver. Можно указывать или IP или доменное имя<br/>\r\n                        '5893' - IP порт удаленного kkmserver<br/>\r\n                        '1' - Номер устройства на удаленном kkmserver<br/>\r\n                        <br/>";
			UnitSettings.Paramerts.Add(paramert12);
			paramert12 = new iUnitSettings.Paramert();
			paramert12.Page = "Параметры";
			paramert12.Group = "Эмуляция чека / маршрутизация команд";
			paramert12.Caption = "Шаблон не фискального чека";
			paramert12.Name = "EmulationCheckForm";
			paramert12.TypeValue = "String";
			paramert12.MasterParameterName = "EmulationCheck";
			paramert12.MasterParameterOperation = "Equal";
			paramert12.MasterParameterValue = "True";
			paramert12.Description = "<br/><br/>\r\n                        Допустимые поля шаблона заголовка/подвала чека (Header/Footer):<br/>\r\n                        0<1,2,3,4>| - номер шрифта + разделитель<br/>\r\n                        'NameOrganization','AddressSettle','DateCheck','KktNumber','RegNumber','FD','CheckNum',\r\n                        'InnOrganization','FnNumber','CashierName','CashierVATIN','AllSum',\r\n                        'ElectronicPayment','AdvancePayment','Credit','CashProvision',\r\n                        'Cash-Sdacha','Cash','Sdacha','Nds22','Nds20','Nds18','Nds10','NdsNo','OSN','FD','FP'<br/>\r\n                        <br/>\r\n                        Допустимые поля шаблона строк (Register):<br/>\r\n                        0<1,2,3,4>| - номер шрифта + разделитель<br/>\r\n                        'Name','Quantity','Price','Ammount','NameSkidka','Skidka','NameTax','NDS'<br/>\r\n                        <br/>\r\n                        При вставке в текст символов '>#10#<' строка при печати выровнеется по центру,<br/>\r\n                        где 10 - это на сколько меньше станет строка ККТ<br/>\r\n                        При вставке в текст в середину строки символов '<#10#>' Левая часть строки будет выравнена<br/>\r\n                        по левому краю, правая по правому, где 10 - это на сколько меньше станет строка ККТ<br/>\r\n                        При вставке в текст в середину строки символов '<#10#>>' Левая часть строки будет выравнена\r\n                        <br/>по правому краю, правая по правому, где 10 - отступ от правого края<br/>\r\n                        При вставке в текст символов '<<->>' будет выведена строка из символов '-' на всю ширину чека<br/>\r\n                        При вставке в текст символа '‗' он будет заменен на пробел<br/>";
			UnitSettings.Paramerts.Add(paramert12);
		}
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков)
		{
			UnitParamets.Add("INN", "");
			if (!SettDr.Paramets.ContainsKey("INN"))
			{
				SettDr.Paramets.Add("INN", "");
			}
			iUnitSettings.Paramert paramert13 = new iUnitSettings.Paramert();
			paramert13.Page = "Параметры";
			paramert13.Group = "Эмуляция чека / маршрутизация команд";
			paramert13.Caption = "INN организации";
			paramert13.Name = "INN";
			paramert13.TypeValue = "String";
			paramert13.Description = "Для поиска нужного принтера.";
			UnitSettings.Paramerts.Add(paramert13);
		}
		UnitParamets.Add("NameDevice", "");
		if (!SettDr.Paramets.ContainsKey("NameDevice"))
		{
			SettDr.Paramets.Add("NameDevice", "");
		}
		iUnitSettings.Paramert paramert14 = new iUnitSettings.Paramert();
		paramert14.Page = "Параметры";
		paramert14.Group = "Прочие настройки";
		paramert14.Caption = "Наименование устройства";
		paramert14.Name = "NameDevice";
		paramert14.TypeValue = "String";
		paramert14.Description = "Не обязательно. Имя устройства которое будет отображатся в списке";
		UnitSettings.Paramerts.Add(paramert14);
		UnitParamets.Add("UnitPassword", "");
		if (!SettDr.Paramets.ContainsKey("UnitPassword"))
		{
			SettDr.Paramets.Add("UnitPassword", "");
		}
		iUnitSettings.Paramert paramert15 = new iUnitSettings.Paramert();
		paramert15.Page = "Параметры";
		paramert15.Group = "Прочие настройки";
		paramert15.Caption = "Пароль на устройство";
		paramert15.Name = "UnitPassword";
		paramert15.TypeValue = "String";
		paramert15.Description = "Не обязательно. Если пароль задан то \"Пользователи\" могут заходить и с этим паролем а не только с паролем, указанным в настройках.";
		UnitSettings.Paramerts.Add(paramert15);
		if (SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор)
		{
			UnitParamets.Add("NoReadQrCode", ExtensionMethods.AsString(Val: false));
			if (!SettDr.Paramets.ContainsKey("NoReadQrCode"))
			{
				SettDr.Paramets.Add("NoReadQrCode", ExtensionMethods.AsString(Val: false));
			}
			iUnitSettings.Paramert paramert16 = new iUnitSettings.Paramert();
			paramert16.Page = "Параметры";
			paramert16.Group = "Прочие настройки";
			paramert16.Caption = "Не читать QR код чеков";
			paramert16.Name = "NoReadQrCode";
			paramert16.TypeValue = "Boolean";
			paramert16.Description = "После регистрации чека не получать данные QR кода.</br>(Фискальную подпись, фискальный номер и дату документа)</br></br>Действие:</br>- довольно длительное</br>- на некоторых ККТ приводит к сбою.";
			UnitSettings.Paramerts.Add(paramert16);
		}
		if (SettDr.TypeDevice.UnitDevice == TypeDevice.enUnitDevice.KkmServerAtol_5)
		{
			UnitParamets.Add("OfdOverWiFi", ExtensionMethods.AsString(Val: false));
			if (!SettDr.Paramets.ContainsKey("OfdOverWiFi"))
			{
				SettDr.Paramets.Add("OfdOverWiFi", ExtensionMethods.AsString(Val: false));
			}
		}
	}

	public void LoadAdditionalActionsFromXML(string ParamsXML)
	{
		UnitActions.Clear();
		if (ParamsXML != "")
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(ParamsXML);
			XmlNode documentElement = xmlDocument.DocumentElement;
			for (int i = 0; i < documentElement.ChildNodes.Count; i++)
			{
				if (documentElement.ChildNodes[i].NodeType != XmlNodeType.Element)
				{
					continue;
				}
				XmlElement obj = (XmlElement)documentElement.ChildNodes[i];
				string key = "";
				string value = "";
				foreach (XmlAttribute attribute in obj.Attributes)
				{
					if (attribute.Name == "Name")
					{
						key = attribute.Value;
					}
					if (attribute.Name == "Caption")
					{
						value = attribute.Value;
					}
				}
				UnitActions.Add(key, value);
			}
		}
		if ((SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор || SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков) && SettDr.Paramets.ContainsKey("EmulationCheck") && SettDr.Paramets["EmulationCheck"].AsBool())
		{
			UnitActions.Add("ClearCheckForm", "Сбросить шаблон эмуляции чека");
		}
	}

	public void LoadParametsItems(XmlElement XmlElement, string Page, string Group)
	{
		for (int i = 0; i < XmlElement.ChildNodes.Count; i++)
		{
			if (XmlElement.ChildNodes[i].NodeType != XmlNodeType.Element)
			{
				continue;
			}
			XmlElement xmlElement = (XmlElement)XmlElement.ChildNodes[i];
			if (xmlElement.Name == "Page")
			{
				if (xmlElement.Attributes != null)
				{
					foreach (XmlAttribute attribute in xmlElement.Attributes)
					{
						if (attribute.Name == "Caption")
						{
							Page = attribute.Value;
						}
					}
				}
				LoadParametsItems(xmlElement, Page, Group);
			}
			if (xmlElement.Name == "Group")
			{
				if (xmlElement.Attributes != null)
				{
					foreach (XmlAttribute attribute2 in xmlElement.Attributes)
					{
						if (attribute2.Name == "Caption")
						{
							Group = attribute2.Value;
						}
					}
				}
				LoadParametsItems(xmlElement, Page, Group);
			}
			if (!(xmlElement.Name == "Parameter"))
			{
				continue;
			}
			iUnitSettings.Paramert paramert = new iUnitSettings.Paramert();
			paramert.Page = Page;
			paramert.Group = Group;
			if (xmlElement.Attributes != null)
			{
				foreach (XmlAttribute attribute3 in xmlElement.Attributes)
				{
					if (attribute3.Name == "Name")
					{
						paramert.Name = attribute3.Value;
					}
					else if (attribute3.Name == "Caption")
					{
						paramert.Caption = attribute3.Value;
					}
					else if (attribute3.Name == "Description")
					{
						paramert.Description = attribute3.Value;
						if (paramert.Description == paramert.Caption)
						{
							paramert.Description = "";
						}
						paramert.Description = paramert.Description.Replace("\n", "\n<br/>");
					}
					else if (attribute3.Name == "Help")
					{
						paramert.Help = attribute3.Value;
					}
					else if (attribute3.Name == "TypeValue")
					{
						paramert.TypeValue = attribute3.Value;
					}
					else if (attribute3.Name == "FieldFormat")
					{
						paramert.FieldFormat = attribute3.Value;
					}
					else if (attribute3.Name == "ReadOnly")
					{
						if (attribute3.Value == "true")
						{
							paramert.ReadOnly = true;
						}
						else
						{
							paramert.ReadOnly = false;
						}
					}
					else if (attribute3.Name == "DefaultValue")
					{
						paramert.DefaultValue = attribute3.Value;
					}
					else if (attribute3.Name == "MasterParameterName")
					{
						paramert.MasterParameterName = attribute3.Value;
					}
					else if (attribute3.Name == "MasterParameterOperation")
					{
						paramert.MasterParameterOperation = attribute3.Value;
					}
					else if (attribute3.Name == "MasterParameterValue")
					{
						paramert.MasterParameterValue = attribute3.Value;
					}
					else if (attribute3.Name == "SaveOnChange" && attribute3.Value == "true")
					{
						paramert.SaveOnChange = true;
					}
				}
			}
			for (int j = 0; j < xmlElement.ChildNodes.Count; j++)
			{
				XmlElement xmlElement2 = (XmlElement)xmlElement.ChildNodes[j];
				for (int k = 0; k < xmlElement2.ChildNodes.Count; k++)
				{
					if (xmlElement2.ChildNodes[k].NodeType != XmlNodeType.Element)
					{
						continue;
					}
					XmlElement xmlElement3 = (XmlElement)xmlElement2.ChildNodes[k];
					iUnitSettings.VauePar vauePar = new iUnitSettings.VauePar();
					if (xmlElement3.Attributes != null)
					{
						foreach (XmlAttribute attribute4 in xmlElement3.Attributes)
						{
							if (attribute4.Name == "Value")
							{
								vauePar.Value = attribute4.Value;
							}
						}
					}
					if (xmlElement3.ChildNodes != null)
					{
						for (int l = 0; l < xmlElement3.ChildNodes.Count; l++)
						{
							vauePar.Caption = ((XmlText)xmlElement3.ChildNodes[l]).Value;
						}
					}
					paramert.VauePars.Add(vauePar);
				}
			}
			UnitSettings.Paramerts.Add(paramert);
			UnitParamets.Add(paramert.Name, CovertTypeValue(paramert.TypeValue, ""));
			if (paramert.DefaultValue != "")
			{
				UnitParamets[paramert.Name] = CovertTypeValue(paramert.TypeValue, paramert.DefaultValue);
			}
		}
	}

	public void CalkPrintOnPage(Unit Unit, DataCommand DataCommand, bool Repot = false)
	{
		if (!SettDr.Paramets.ContainsKey("PrintOnPage"))
		{
			if (!DataCommand.NotPrint.HasValue)
			{
				DataCommand.NotPrint = false;
			}
			return;
		}
		switch (SettDr.Paramets["PrintOnPage"])
		{
		case "":
			if (!DataCommand.NotPrint.HasValue)
			{
				DataCommand.NotPrint = false;
			}
			break;
		case "NotPrint":
			if (!DataCommand.NotPrint.HasValue)
			{
				DataCommand.NotPrint = true;
			}
			break;
		case "NotPrint+Report":
			if (Repot)
			{
				DataCommand.NotPrint = false;
			}
			else if (!DataCommand.NotPrint.HasValue)
			{
				DataCommand.NotPrint = true;
			}
			break;
		case "AlwaysPrint":
			DataCommand.NotPrint = false;
			break;
		case "AlwaysNotPrint":
			DataCommand.NotPrint = true;
			break;
		case "AlwaysNotPrint+Report":
			if (Repot)
			{
				DataCommand.NotPrint = false;
			}
			else
			{
				DataCommand.NotPrint = true;
			}
			break;
		}
	}

	public string GetEmulationCheckForm()
	{
		return "#Header{\r\n1|>#2#<'NameOrganization'\r\n4|>#2#<'AddressSettle'\r\n0|>#2#<ДОБРО ПОЖАЛОВАТЬ!\r\n0|‗\r\n2|>#2#<'TypeCheck'\r\n0|РН ККТ:'RegNumber'<#0#>'DateCheck'\r\n0|ЗН ККТ:'KktNumber'<#0#>СМЕНА:'SessionNumber' ЧЕК:'CheckNum'\r\n0|ФН:'FnNumber'<#0#>ИНН:'InnOrganization'\r\n0|КАССИР:<#0#>'CashierName'\r\n0|ИНН кассира:<#0#>'CashierVATIN'\r\n0|Место расчетов:<#0#>'PlaceSettle'\r\n4|Эл.адрес отправителя:<#0#>'SenderEmail'\r\n4|Сайт ФНС:<#0#>www.nalog.gov.ru\r\n4|Тел/емайл покупателя:<#0#>'ClientAddress'\r\n4|Наименование покупателя:<#0#>'ClientInfo'\r\n4|ИНН покупателя:<#0#>'ClientINN'\r\n4|Доп.атрибут:<#0#>'AdditionalAttribute'\r\n4|'UserAttributeName':<#0#>'UserAttributeValue'\r\n0|<<->>\r\n#Header}\r\n#Register{\r\n0|'Name'\r\n0| ‗<#0#>'Quantity' X 'Price'\r\n0| ‗<#0#>='Ammount'\r\n4|'NameSkidka'<#0#>='Skidka'\r\n4|Доп.атрибут:<#0#>'AdditionalAttribute'\r\n4|НДС 'NameTax'<#0#>='NDS'\r\n#Register}\r\n#Footer{\r\n0| <<->>\r\n1|ИТОГ<#0#>='AllSum'\r\n0|‗НАЛИЧНЫМИ<#0#>='Cash-Sdacha'\r\n0|‗БЕЗНАЛИЧНЫМИ<#0#>='ElectronicPayment'\r\n0|‗ЗАЧЕТ АВАНСА<#0#>='AdvancePayment'\r\n0|‗КРЕДИТ<#0#>='Credit'\r\n0|‗ВСТРЕЧНОЕ ПРЕДСТАВЛЕНИЕ<#0#>='CashProvision'\r\n0|ПОЛУЧЕНО:'PrintPoluceno'\r\n0|‗БЕЗНАЛИЧНЫМИ<#0#>='ElectronicPayment'\r\n0|‗НАЛИЧНЫМИ<#0#>='Cash'\r\n0|Сдача:<#0#>='Sdacha'\r\n0|СУММА НДС 22%:<#0#>='Sum22'\r\n0|СУММА НДС 20%:<#0#>='Sum20'\r\n0|СУММА НДС 18%:<#0#>='Sum18'\r\n0|СУММА НДС 10%:<#0#>='Sum10'\r\n0|СУММА НДС 7%:<#0#>='Sum7'\r\n0|СУММА НДС 5%:<#0#>='Sum5'\r\n0|СУММА НДС 0%:<#0#>='Sum0'\r\n0|СУММА БЕЗ НДС:<#0#>='SumNo'\r\n0| <<->>\r\n0|СНО:'OSN'<#0#>ФД:'FD'\r\n0|Дата:'FDate'<#0#>ФП:'FP'\r\n#Footer}";
	}

	public DataCommand GetNoFiscalCheck(DataCommand Command, bool IsRoute, RezultCommandKKm RezultCommand, string Template = null, int? SessionNumberN = null, int? CheckNumN = null, DateTime? CheckDateN = null, string CheckFPN = null, string CheckFDN = null)
	{
		if (Template == null)
		{
			Template = SettDr.Paramets["EmulationCheckForm"];
		}
		int num = 0;
		int num2 = 0;
		DateTime checkDate = DateTime.Now;
		string checkFP = "";
		string checkFD = "";
		if (!IsRoute)
		{
			if (SessionNumberN.HasValue)
			{
				num = SessionNumberN.Value;
				num2 = CheckNumN.Value;
				checkDate = CheckDateN.Value;
				checkFP = CheckFPN;
				checkFD = CheckFDN;
			}
			else
			{
				num = DateTime.Now.DayOfYear;
				num2 = ++Kkm.EmulationCheckNum;
				checkFD = num2.ToString();
				checkDate = DateTime.Now;
			}
		}
		else
		{
			string[] array = RezultCommand.QRCode.Split('&');
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split('=');
				switch (array2[0])
				{
				case "t":
					checkDate = DateTime.ParseExact(array2[1], "yyyyMMddTHHmm", CultureInfo.InvariantCulture);
					break;
				case "fp":
					checkFP = array2[1];
					break;
				case "i":
					checkFD = array2[1];
					break;
				}
			}
			num = RezultCommand.SessionNumber;
			num2 = RezultCommand.SessionCheckNumber;
		}
		DataCommand dataCommand = new DataCommand();
		dataCommand.Command = "PrintDocument";
		dataCommand.IsFiscalCheck = false;
		dataCommand.NumDevice = SettDr.NumDevice;
		dataCommand.NotClearText = Command.NotClearText;
		dataCommand.KeySubLicensing = Command.KeySubLicensing;
		List<DataCommand.CheckString> list = new List<DataCommand.CheckString>();
		string text = Template.Substring(Template.IndexOf("#Header{") + "#Header{\n\r".Length, Template.IndexOf("#Header}") - Template.IndexOf("#Header{") - "#Header{\r\n".Length);
		string text2 = Template.Substring(Template.IndexOf("#Register{") + "#Register{\n\r".Length, Template.IndexOf("#Register}") - Template.IndexOf("#Register{") - "#Register{\n\r".Length);
		string text3 = Template.Substring(Template.IndexOf("#Footer{") + "#Footer{\n\r".Length, Template.IndexOf("#Footer}") - Template.IndexOf("#Footer{") - "#Footer{\n\r".Length);
		new Random().Next(1000000000, int.MaxValue);
		string[] array3 = text.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		string[] array4 = text3.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
		DataCommand.CheckString[] checkStrings;
		for (int j = 0; j < 3; j++)
		{
			switch (j)
			{
			case 0:
			{
				List<string> list4 = array3.ToList();
				FillHeaderFooter(list4, Command, num, num2, checkDate, checkFD, checkFP);
				foreach (string item in list4)
				{
					string[] array8 = item.Split(new string[1] { "|" }, StringSplitOptions.None);
					DataCommand.CheckString checkString7 = new DataCommand.CheckString();
					checkString7.PrintText = new DataCommand.PrintString();
					checkString7.PrintText.Font = int.Parse(array8[0]);
					checkString7.PrintText.Text = array8[1];
					list.Add(checkString7);
				}
				break;
			}
			case 1:
				checkStrings = Command.CheckStrings;
				foreach (DataCommand.CheckString Str in checkStrings)
				{
					if (Str.Register != null)
					{
						string[] array6 = text2.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
						decimal num3 = Str.Register.Quantity * Str.Register.Price - Str.Register.Amount;
						decimal num4 = default(decimal);
						if (Str.Register.Tax == 10m)
						{
							num4 = Str.Register.Amount / 110m * 10m;
						}
						else if (Str.Register.Tax == 18m)
						{
							num4 = Str.Register.Amount / 118m * 18m;
						}
						else if (Str.Register.Tax == 22m)
						{
							num4 = Str.Register.Amount / 122m * 22m;
						}
						else if (Str.Register.Tax == 20m)
						{
							num4 = Str.Register.Amount / 120m * 20m;
						}
						else if (Str.Register.Tax == 122m)
						{
							num4 = Str.Register.Amount / 122m * 22m;
						}
						else if (Str.Register.Tax == 120m)
						{
							num4 = Str.Register.Amount / 120m * 20m;
						}
						else if (Str.Register.Tax == 118m)
						{
							num4 = Str.Register.Amount / 118m * 18m;
						}
						else if (Str.Register.Tax >= -1m)
						{
							num4 = Str.Register.Amount / 120m * 20m;
						}
						else if (Str.Register.Tax == 5m)
						{
							num4 = Str.Register.Amount / 105m * 5m;
						}
						else if (Str.Register.Tax == 105m)
						{
							num4 = Str.Register.Amount / 105m * 5m;
						}
						else if (Str.Register.Tax == 7m)
						{
							num4 = Str.Register.Amount / 107m * 7m;
						}
						else if (Str.Register.Tax == 107m)
						{
							num4 = Str.Register.Amount / 107m * 7m;
						}
						string text4 = null;
						if (Str.Register.GoodCodeData != null)
						{
							if (RezultCommand.MarkingCodeValidation != null)
							{
								RezultCommandKKm.tMarkingCodeValidation tMarkingCodeValidation = RezultCommand.MarkingCodeValidation.Find((RezultCommandKKm.tMarkingCodeValidation x) => x.BarCode == Str.Register.GoodCodeData.BarCode);
								text4 = ((tMarkingCodeValidation == null || tMarkingCodeValidation.ValidationKKT.DecryptionResult.IndexOf("]") == -1 || tMarkingCodeValidation.ValidationKKT.DecryptionResult.IndexOf("]") > 5) ? " [M]" : tMarkingCodeValidation.ValidationKKT.DecryptionResult.Substring(0, tMarkingCodeValidation.ValidationKKT.DecryptionResult.IndexOf("]") + 1));
							}
							else
							{
								text4 = " [M]";
							}
						}
						for (int num5 = 0; num5 < array6.Length; num5++)
						{
							array6[num5] = array6[num5].Replace("'Name'", Str.Register.Name + text4);
							array6[num5] = array6[num5].Replace("'Quantity'", Str.Register.Quantity.ToString("0.000", CultureInfo.InvariantCulture));
							array6[num5] = array6[num5].Replace("'Price'", Str.Register.Price.ToString("0.00", CultureInfo.InvariantCulture));
							array6[num5] = array6[num5].Replace("'Ammount'", (Str.Register.Quantity * Str.Register.Price).ToString("0.00", CultureInfo.InvariantCulture));
							if (num3 > 0m)
							{
								array6[num5] = array6[num5].Replace("'NameSkidka'", "СКИДКА");
								array6[num5] = array6[num5].Replace("'Skidka'", num3.ToString("0.00", CultureInfo.InvariantCulture));
							}
							else if (num3 < 0m)
							{
								array6[num5] = array6[num5].Replace("'NameSkidka'", "НАЦЕНКА");
								array6[num5] = array6[num5].Replace("'Skidka'", (-num3).ToString("0.00", CultureInfo.InvariantCulture));
							}
							if (num4 > 0m && !(Str.Register.Tax == -1m))
							{
								array6[num5] = array6[num5].Replace("'NameTax'", Str.Register.Tax + "%");
								array6[num5] = array6[num5].Replace("'NDS'", num4.ToString("0.00", CultureInfo.InvariantCulture));
							}
							if (Str.Register.AdditionalAttribute != null)
							{
								array6[num5] = array6[num5].Replace("'AdditionalAttribute'", Str.Register.AdditionalAttribute);
							}
						}
						List<string> list3 = array6.ToList();
						for (int num6 = list3.Count - 1; num6 > 0; num6--)
						{
							if (list3[num6].IndexOf("'NameTax'") != -1)
							{
								list3.RemoveAt(num6);
							}
							else if (list3[num6].IndexOf("'NameSkidka'") != -1)
							{
								list3.RemoveAt(num6);
							}
							else if (list3[num6].IndexOf("'KIZ'") != -1)
							{
								list3.RemoveAt(num6);
							}
							else if (list3[num6].IndexOf("'AdditionalAttribute'") != -1)
							{
								list3.RemoveAt(num6);
							}
						}
						foreach (string item2 in list3)
						{
							string[] array7 = item2.Split(new string[1] { "|" }, StringSplitOptions.None);
							DataCommand.CheckString checkString2 = new DataCommand.CheckString();
							checkString2.PrintText = new DataCommand.PrintString();
							checkString2.PrintText.Font = int.Parse(array7[0]);
							checkString2.PrintText.Text = array7[1];
							list.Add(checkString2);
						}
					}
					if (Str.PrintText != null)
					{
						DataCommand.CheckString checkString3 = new DataCommand.CheckString();
						checkString3.PrintText = new DataCommand.PrintString();
						checkString3.PrintText.Font = Str.PrintText.Font;
						checkString3.PrintText.Intensity = Str.PrintText.Intensity;
						checkString3.PrintText.Text = Str.PrintText.Text;
						list.Add(checkString3);
					}
					if (Str.BarCode != null)
					{
						DataCommand.CheckString checkString4 = new DataCommand.CheckString();
						checkString4.BarCode = new DataCommand.PrintBarcode();
						checkString4.BarCode.BarcodeType = Str.BarCode.BarcodeType;
						checkString4.BarCode.Barcode = Str.BarCode.Barcode;
						list.Add(checkString4);
					}
					if (Str.PrintImage != null)
					{
						DataCommand.CheckString checkString5 = new DataCommand.CheckString();
						checkString5.PrintImage = new DataCommand.PrintImage();
						checkString5.PrintImage.Image = Str.PrintImage.Image;
						list.Add(checkString5);
					}
				}
				if (Command.Command == "RegisterCheck" && (Command.TypeCheck == 2 || Command.TypeCheck == 12) && Command.Amount != 0m)
				{
					DataCommand.CheckString checkString6 = new DataCommand.CheckString();
					checkString6.PrintText = new DataCommand.PrintString();
					checkString6.PrintText.Font = 0;
					checkString6.PrintText.Intensity = 0;
					checkString6.PrintText.Text = "Сумма расчетов:<#0#>" + Command.Amount.ToString("### ### ### ###.00");
					list.Add(checkString6);
				}
				break;
			case 2:
			{
				List<string> list2 = array4.ToList();
				FillHeaderFooter(list2, Command, num, num2, checkDate, checkFD, checkFP);
				foreach (string item3 in list2)
				{
					string[] array5 = item3.Split(new string[1] { "|" }, StringSplitOptions.None);
					DataCommand.CheckString checkString = new DataCommand.CheckString();
					checkString.PrintText = new DataCommand.PrintString();
					checkString.PrintText.Font = int.Parse(array5[0]);
					checkString.PrintText.Text = array5[1];
					list.Add(checkString);
				}
				break;
			}
			}
		}
		decimal num7 = default(decimal);
		checkStrings = Command.CheckStrings;
		foreach (DataCommand.CheckString checkString8 in checkStrings)
		{
			if (checkString8.Register != null)
			{
				num7 += checkString8.Register.Amount;
			}
		}
		if (Command.Command == "RegisterCheck" && (Command.TypeCheck == 0 || Command.TypeCheck == 1 || Command.TypeCheck == 10 || Command.TypeCheck == 11))
		{
			string text5 = "";
			text5 = (IsRoute ? RezultCommand.QRCode : ("t=" + DateTime.Now.ToString("yyyyMMddTHHmm") + "&s=" + num7.ToString("0.00").Replace(',', '.') + "&fn=" + Kkm.Fn_Number + "&i=" + num2.ToString("D0") + "&n=1"));
			if (text5 != "")
			{
				DataCommand.PrintBarcode printBarcode = new DataCommand.PrintBarcode();
				printBarcode.Barcode = text5;
				printBarcode.BarcodeType = "QR";
				DataCommand.CheckString checkString9 = new DataCommand.CheckString();
				checkString9.BarCode = printBarcode;
				list.Add(checkString9);
			}
		}
		dataCommand.CheckStrings = list.ToArray();
		return dataCommand;
	}

	public void FillHeaderFooter(List<string> Arr, DataCommand Command, int SessionNumber, int CheckNum, DateTime CheckDate, string CheckFD, string CheckFP)
	{
		decimal num = default(decimal);
		decimal num2 = default(decimal);
		decimal num3 = default(decimal);
		decimal num4 = default(decimal);
		decimal num5 = default(decimal);
		decimal num6 = default(decimal);
		decimal num7 = default(decimal);
		decimal num8 = default(decimal);
		decimal num9 = -1m;
		DataCommand.CheckString[] checkStrings = Command.CheckStrings;
		foreach (DataCommand.CheckString checkString in checkStrings)
		{
			if (checkString.Register != null)
			{
				num += checkString.Register.Amount;
				if (checkString.Register.Tax == 22m)
				{
					num2 += checkString.Register.Amount;
				}
				else if (checkString.Register.Tax == 20m)
				{
					num3 += checkString.Register.Amount;
				}
				else if (checkString.Register.Tax == 18m)
				{
					num4 += checkString.Register.Amount;
				}
				else if (checkString.Register.Tax == 10m)
				{
					num5 += checkString.Register.Amount;
				}
				else if (checkString.Register.Tax == 0m)
				{
					num8 += checkString.Register.Amount;
				}
				else if (checkString.Register.Tax == -1m)
				{
					num9 = ((num9 == -1m) ? 0m : num9) + checkString.Register.Amount;
				}
				else if (checkString.Register.Tax == 5m)
				{
					num6 += checkString.Register.Amount;
				}
				else if (checkString.Register.Tax == 7m)
				{
					num7 += checkString.Register.Amount;
				}
			}
		}
		num2 += Command.SumTax22;
		num3 += Command.SumTax20;
		num5 += Command.SumTax10;
		num4 += Command.SumTax18;
		num6 += Command.SumTax5;
		num7 += Command.SumTax7;
		num8 += Command.SumTax0;
		num9 += Command.SumTaxNone;
		num2 = num2 / 122m * 22m;
		num3 = num3 / 120m * 20m;
		num5 = num5 / 110m * 10m;
		num4 = num4 / 180m * 18m;
		num8 += Command.SumTax0;
		num9 += Command.SumTaxNone;
		decimal cash = Command.Cash;
		decimal num10 = Command.ElectronicPayment + Command.AdvancePayment + Command.Credit + Command.CashProvision + Command.Cash - num;
		if (Command.Command == "RegisterCheck")
		{
			if (Command.TypeCheck != 0 && Command.TypeCheck != 1 && Command.TypeCheck != 10 && Command.TypeCheck != 11)
			{
				num10 = default(decimal);
			}
			if (Command.TypeCheck == 2 || Command.TypeCheck == 12)
			{
				num = num + Command.Cash + Command.ElectronicPayment + Command.AdvancePayment + Command.Credit + Command.CashProvision;
			}
		}
		else
		{
			num10 = default(decimal);
			if (Command.Command == "DepositingCash")
			{
				cash += Command.Amount;
				num += Command.Amount;
			}
			else if (Command.Command == "PaymentCash")
			{
				cash += Command.Amount;
				num += Command.Amount;
			}
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("", "Общая ОСН");
		dictionary.Add("0", "Общая ОСН");
		dictionary.Add("1", "УСН доход");
		dictionary.Add("2", "УСН доход-расход");
		dictionary.Add("3", "ЕНВД");
		dictionary.Add("4", "ЕСН");
		dictionary.Add("5", "Патент");
		for (int j = 0; j < Arr.Count; j++)
		{
			if (Command.Command == "RegisterCheck")
			{
				switch (Command.TypeCheck)
				{
				case 0:
					Arr[j] = Arr[j].Replace("'TypeCheck'", "ПРИХОД");
					break;
				case 1:
					Arr[j] = Arr[j].Replace("'TypeCheck'", "ВОЗВРАТ ПРИХОДА");
					break;
				case 10:
					Arr[j] = Arr[j].Replace("'TypeCheck'", "РАСХОД");
					break;
				case 11:
					Arr[j] = Arr[j].Replace("'TypeCheck'", "ВОЗВРАТ РАСХОДА");
					break;
				case 2:
					Arr[j] = Arr[j].Replace("'TypeCheck'", "КОРРЕКТИРОВКА ПРИХОД");
					break;
				case 12:
					Arr[j] = Arr[j].Replace("'TypeCheck'", "КОРРЕКТИРОВКА РАСХОД");
					break;
				}
			}
			else if (Command.Command == "DepositingCash")
			{
				Arr[j] = Arr[j].Replace("'TypeCheck'", "ВНЕСЕНИЕ СРЕДСТВ");
			}
			else if (Command.Command == "PaymentCash")
			{
				Arr[j] = Arr[j].Replace("'TypeCheck'", "ИЗЪЯТИЕ СРЕДСТВ");
			}
			else if (Command.Command == "OpenShift")
			{
				Arr[j] = Arr[j].Replace("'TypeCheck'", "ОТКРЫТИЕ СМЕНЫ");
			}
			else if (Command.Command == "CloseShift")
			{
				Arr[j] = Arr[j].Replace("'TypeCheck'", "ЗАКРЫТИЕ СМЕНЫ");
			}
			else if (Command.Command == "ZReport")
			{
				Arr[j] = Arr[j].Replace("'TypeCheck'", "ЗАКРЫТИЕ СМЕНЫ");
			}
			Arr[j] = Arr[j].Replace("'NameOrganization'", Kkm.Organization);
			Arr[j] = Arr[j].Replace("'AddressSettle'", Kkm.AddressSettle);
			if (Kkm.PlaceSettle != null && Kkm.PlaceSettle != "")
			{
				Arr[j] = Arr[j].Replace("'PlaceSettle'", Kkm.PlaceSettle);
			}
			if (Kkm.SenderEmail != null && Kkm.SenderEmail != "")
			{
				Arr[j] = Arr[j].Replace("'SenderEmail'", Kkm.SenderEmail);
			}
			if (Command.ClientAddress != null && Command.ClientAddress != "")
			{
				Arr[j] = Arr[j].Replace("'ClientAddress'", Command.ClientAddress);
			}
			if (Command.ClientInfo != null && Command.ClientInfo != "")
			{
				Arr[j] = Arr[j].Replace("'ClientInfo'", Command.ClientInfo);
			}
			if (Command.ClientINN != null && Command.ClientINN != "")
			{
				Arr[j] = Arr[j].Replace("'ClientINN'", Command.ClientINN);
			}
			if (Command.AdditionalAttribute != null && Command.AdditionalAttribute != "")
			{
				Arr[j] = Arr[j].Replace("'AdditionalAttribute'", Command.AdditionalAttribute);
			}
			if (Command.UserAttribute != null)
			{
				if (Command.UserAttribute.Name != null)
				{
					Arr[j] = Arr[j].Replace("'UserAttributeName'", Command.UserAttribute.Name);
				}
				if (Command.UserAttribute.Value != null)
				{
					Arr[j] = Arr[j].Replace("'UserAttributeValue'", Command.UserAttribute.Value);
				}
			}
			Arr[j] = Arr[j].Replace("'RegNumber'", Kkm.RegNumber);
			Arr[j] = Arr[j].Replace("'DateCheck'", CheckDate.ToString("dd.MM.yyyy HH:mm"));
			Arr[j] = Arr[j].Replace("'KktNumber'", Kkm.NumberKkm);
			Arr[j] = Arr[j].Replace("'SessionNumber'", SessionNumber.ToString());
			Arr[j] = Arr[j].Replace("'CheckNum'", CheckNum.ToString());
			Arr[j] = Arr[j].Replace("'InnOrganization'", Kkm.INN);
			Arr[j] = Arr[j].Replace("'FnNumber'", Kkm.Fn_Number);
			Arr[j] = Arr[j].Replace("'CashierName'", Command.CashierName);
			if (Command.CashierVATIN != null && Command.CashierVATIN != "")
			{
				Arr[j] = Arr[j].Replace("'CashierVATIN'", Command.CashierVATIN);
			}
			if (num != 0m || Command.Command == "RegisterCheck")
			{
				Arr[j] = Arr[j].Replace("'AllSum'", num.ToString("0.00", CultureInfo.InvariantCulture));
			}
			if (Command.ElectronicPayment > 0m)
			{
				Arr[j] = Arr[j].Replace("'ElectronicPayment'", Command.ElectronicPayment.ToString("0.00", CultureInfo.InvariantCulture));
				Arr[j] = Arr[j].Replace("'PrintPoluceno'", "");
			}
			if (Command.AdvancePayment > 0m)
			{
				Arr[j] = Arr[j].Replace("'AdvancePayment'", Command.AdvancePayment.ToString("0.00", CultureInfo.InvariantCulture));
			}
			if (Command.Credit > 0m)
			{
				Arr[j] = Arr[j].Replace("'Credit'", Command.Credit.ToString("0.00", CultureInfo.InvariantCulture));
			}
			if (Command.CashProvision > 0m)
			{
				Arr[j] = Arr[j].Replace("'CashProvision'", Command.CashProvision.ToString("0.00", CultureInfo.InvariantCulture));
			}
			if (cash - num10 != 0m)
			{
				Arr[j] = Arr[j].Replace("'Cash-Sdacha'", (cash - num10).ToString("0.00", CultureInfo.InvariantCulture));
				Arr[j] = Arr[j].Replace("'PrintPoluceno'", "");
			}
			if (cash > 0m)
			{
				Arr[j] = Arr[j].Replace("'Cash'", cash.ToString("0.00", CultureInfo.InvariantCulture));
				Arr[j] = Arr[j].Replace("'PrintPoluceno'", "");
			}
			if (num10 > 0m)
			{
				Arr[j] = Arr[j].Replace("'Sdacha'", num10.ToString("0.00", CultureInfo.InvariantCulture));
			}
			if (num2 > 0m)
			{
				Arr[j] = Arr[j].Replace("'Sum22'", num2.ToString("0.00", CultureInfo.InvariantCulture));
			}
			if (num3 > 0m)
			{
				Arr[j] = Arr[j].Replace("'Sum20'", num3.ToString("0.00", CultureInfo.InvariantCulture));
			}
			if (num4 > 0m)
			{
				Arr[j] = Arr[j].Replace("'Sum18'", num4.ToString("0.00", CultureInfo.InvariantCulture));
			}
			if (num5 > 0m)
			{
				Arr[j] = Arr[j].Replace("'Sum10'", num5.ToString("0.00", CultureInfo.InvariantCulture));
			}
			if (num8 > 0m)
			{
				Arr[j] = Arr[j].Replace("'Sum0'", num8.ToString("0.00", CultureInfo.InvariantCulture));
			}
			if (num9 >= 0m)
			{
				Arr[j] = Arr[j].Replace("'SumNo'", num9.ToString("0.00", CultureInfo.InvariantCulture));
			}
			Arr[j] = Arr[j].Replace("'OSN'", dictionary[Command.TaxVariant]);
			Arr[j] = Arr[j].Replace("'FDate'", CheckDate.ToString("dd.MM.yyyy HH:mm"));
			Arr[j] = Arr[j].Replace("'FD'", CheckFD);
			Arr[j] = Arr[j].Replace("'FP'", CheckFP);
			Arr[j] = Arr[j].Replace("'NumCheck'", CheckNum.ToString("0000"));
		}
		for (int num11 = Arr.Count - 1; num11 > 0; num11--)
		{
			if (Arr[num11].IndexOf("'") != -1)
			{
				Arr.RemoveAt(num11);
			}
		}
	}

	public void LoadParametsFromSettings(Global.DeviceSettings SettDr)
	{
		foreach (KeyValuePair<string, string> unitParamet in UnitParamets)
		{
			if (!SettDr.Paramets.ContainsKey(unitParamet.Key))
			{
				SettDr.Paramets.Add(unitParamet.Key, UnitParamets[unitParamet.Key]);
			}
			else if (unitParamet.Value.GetType() != SettDr.Paramets[unitParamet.Key].GetType())
			{
				SettDr.Paramets[unitParamet.Key] = CovertTypeValue(UnitParamets[unitParamet.Key], SettDr.Paramets[unitParamet.Key]);
			}
		}
		foreach (KeyValuePair<string, string> paramet in SettDr.Paramets)
		{
			if (UnitParamets.ContainsKey(paramet.Key))
			{
				UnitParamets[paramet.Key] = SettDr.Paramets[paramet.Key];
			}
		}
	}

	public string CovertTypeValue(string Type, string Value)
	{
		return Type switch
		{
			"String" => Value, 
			"Number" => Value.AsDouble().AsString(), 
			"Boolean" => Value.AsBool().AsString(), 
			_ => "", 
		};
	}

	public iUnitSettings.Paramert FindUnitSettingsParam(string Name, bool FindOnCaption = false)
	{
		foreach (iUnitSettings.Paramert paramert in UnitSettings.Paramerts)
		{
			if (paramert.Name == Name)
			{
				return paramert;
			}
		}
		if (FindOnCaption)
		{
			foreach (iUnitSettings.Paramert paramert2 in UnitSettings.Paramerts)
			{
				if (paramert2.Caption == Name)
				{
					return paramert2;
				}
			}
		}
		if (Name == "Port")
		{
			foreach (iUnitSettings.Paramert paramert3 in UnitSettings.Paramerts)
			{
				if (paramert3.Caption == "Порт")
				{
					return paramert3;
				}
			}
		}
		return null;
	}

	public static string GetPringString(string Text, int Len, string NewLine = null)
	{
		string text = "";
		if (Len == 0)
		{
			Len = 36;
		}
		if (Text == null)
		{
			Text = "";
		}
		int num = Text.IndexOf("<<");
		int num2 = Text.IndexOf(">>");
		int num3 = Text.IndexOf(">#");
		int num4 = Text.IndexOf("#<");
		int num5 = Text.IndexOf("<#");
		int num6 = Text.IndexOf("#>");
		int num7 = Text.IndexOf("#>>");
		if (num != -1 && num2 != -1 && num2 - num == 3)
		{
			return "".PadRight(Len, Text.Substring(num + 2, 1)[0]).Replace('_', ' ');
		}
		if (num3 != -1 && num4 != -1 && num4 - num3 >= 3 && num4 - num3 <= 4)
		{
			string text2 = (Text.Substring(0, num3) + Text.Substring(num4 + 2)).Trim();
			int len = Len - 6;
			try
			{
				len = Len - int.Parse(Text.Substring(num3 + 2, num4 - num3 - 2));
			}
			catch
			{
			}
			string[] array = SplitStringOnWord(text2, len);
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = "".PadLeft((Len - array[i].Length) / 2, ' ') + array[i];
				array[i] = array[i].PadRight(Len, ' ');
			}
			string[] array2 = array;
			foreach (string text3 in array2)
			{
				text += text3;
			}
		}
		else if (num5 != -1 && num6 != -1 && num6 - num5 >= 3 && num6 - num5 <= 4)
		{
			Text.IndexOf("<#!#>");
			string text4 = Text.Substring(0, num5).Trim();
			string text5 = "";
			text5 = ((num7 == -1) ? Text.Substring(num6 + 2).Trim() : Text.Substring(num7 + 3).Trim());
			int num8 = Len - 6;
			try
			{
				num8 = Len - int.Parse(Text.Substring(num5 + 2, num6 - num5 - 2));
			}
			catch
			{
			}
			string[] array3 = SplitStringOnWord(text4, num8);
			string[] array4 = SplitStringOnWord(text5, num8);
			int num9 = 0;
			for (int k = 0; k < array3.Length; k++)
			{
				if (k != array3.Length - 1)
				{
					text = ((num7 == -1) ? (text + array3[k].TrimStart().PadRight(Len)) : (text + array3[k].TrimStart().PadLeft(num8).PadRight(Len)));
					continue;
				}
				string text6 = "";
				if (num7 != -1)
				{
					text6 = array3[k].Trim().PadLeft(num8);
					text += text6;
					num9 = Len - text6.Length;
				}
				else
				{
					text6 = array3[k].Trim();
					text += text6;
					num9 = num8 - text6.Length;
				}
			}
			for (int l = 0; l < array4.Length; l++)
			{
				string text7 = array4[l].Trim();
				if (NewLine != null && num9 < text7.Length)
				{
					text += NewLine;
					num9 = Len;
				}
				text = ((l != 0) ? (text + text7) : ((num7 == -1) ? ((num9 - text7.Length < 0) ? (text + "".PadRight(Len) + text7) : ((num9 - text7.Length < 0) ? (text.PadRight(Len) + text7.PadLeft(num8)) : (text + "".PadRight(num9 - text7.Length) + text7))) : ((num9 + text7.Length > Len || num9 - text7.Length < 0) ? (text.PadRight(Len) + text7.PadLeft(Len)) : (text + "".PadRight(num9 - text7.Length) + text7))));
			}
		}
		else
		{
			text = Text;
		}
		return text.Replace('‗', ' ');
	}

	public static string[] SplitStringOnWord(string Text, int Len)
	{
		char[] array = Text.ToCharArray();
		string[] array2 = new string[1];
		bool flag = false;
		char[] array3 = array;
		foreach (char c in array3)
		{
			if (c == ' ')
			{
				if (!flag)
				{
					array2[array2.Length - 1] = array2[array2.Length - 1] + c;
					continue;
				}
				Array.Resize(ref array2, array2.Length + 1);
				array2[array2.Length - 1] = array2[array2.Length - 1] + c;
			}
			else
			{
				flag = true;
				array2[array2.Length - 1] = array2[array2.Length - 1] + c;
			}
		}
		string[] array4 = new string[0];
		string text = "";
		string[] array5 = array2;
		foreach (string text2 in array5)
		{
			string text3 = text + text2;
			if (text3.Length <= Len)
			{
				text = text3;
				continue;
			}
			Array.Resize(ref array4, array4.Length + 1);
			array4[array4.Length - 1] = text;
			if (text2.Length <= Len)
			{
				text = text2.TrimStart();
				continue;
			}
			text = text2.Substring(0, Len).TrimStart();
			Array.Resize(ref array4, array4.Length + 1);
			array4[array4.Length - 1] = text;
			text = text2.Substring(Len).TrimStart();
		}
		if (text != "")
		{
			Array.Resize(ref array4, array4.Length + 1);
			array4[array4.Length - 1] = text;
		}
		return array4;
	}

	public string FormatSlipCheck(string Value)
	{
		string[] array = Value.Replace("\r\n", "\r").Split("\r".ToCharArray());
		string text = "";
		string[] array2 = array;
		foreach (string text2 in array2)
		{
			if (text2.Length > Kkm.PrintingWidth)
			{
				string text3 = text2.Substring(0, Kkm.PrintingWidth);
				string text4 = text2.Substring(Kkm.PrintingWidth);
				text = text + text3 + "\r\n";
				text = text + text4 + "\r\n";
			}
			else
			{
				text = text + text2 + "\r\n";
			}
		}
		return text;
	}

	public async Task PringArrayString(string Value, int NumberCopies)
	{
		string text = Value.Replace("\r\n", "\r");
		string[] Str = text.Split("\r".ToCharArray());
		new Task(async delegate
		{
			DataCommand dataCommand = new DataCommand
			{
				Command = "RegisterCheck",
				IsFiscalCheck = false,
				NotPrint = false,
				NumDevice = SettDr.NumDevice,
				NumberCopies = NumberCopies - 1
			};
			if (UnitParamets.ContainsKey("UnitPassword"))
			{
				dataCommand.UnitPassword = UnitParamets["UnitPassword"];
			}
			dataCommand.CheckStrings = new DataCommand.CheckString[Str.Length];
			int num = 0;
			string[] array = Str;
			foreach (string text2 in array)
			{
				dataCommand.CheckStrings[num] = new DataCommand.CheckString();
				dataCommand.CheckStrings[num].PrintText = new DataCommand.PrintString();
				dataCommand.CheckStrings[num].PrintText.Font = 0;
				dataCommand.CheckStrings[num].PrintText.Intensity = 0;
				dataCommand.CheckStrings[num].PrintText.Text = text2;
				num++;
			}
			string textCommand = JsonConvert.SerializeObject(dataCommand);
			await Global.UnitManager.AddCommand(dataCommand, "sync", textCommand);
		}).Start();
	}

	private async Task<DataCommand> CreateCommandSlip(string Text, int PrintingWidth = 0)
	{
		if (PrintingWidth == 0)
		{
			PrintingWidth = Kkm.PrintingWidth;
		}
		string[] SlipStrings = Text.Replace("\r", "").Split('\n');
		int num = 0;
		string[] array = SlipStrings;
		foreach (string text in array)
		{
			num = Math.Max(num, text.Length - 1);
		}
		string text2;
		if (PrintingWidth - num > 0)
		{
			text2 = "".PadLeft((PrintingWidth - num) / 2);
		}
		else
		{
			await Global.Logers.AddError("Внимание!", "Ширина слип-чека превышает ширину печати ККТ!", "Рекомендация!", "Ширина слип-чека превышает ширину печати ККТ.\r\nУменьшите ширину слип-чека в настройках эквайрингово терминала!");
			text2 = "";
		}
		DataCommand dataCommand = new DataCommand();
		dataCommand.NumDevice = SettDr.NumDevice;
		dataCommand.Command = "RegisterCheck";
		dataCommand.IsFiscalCheck = false;
		dataCommand.IdCommand = Guid.NewGuid().ToString();
		DataCommand.CheckString[] array2 = new DataCommand.CheckString[SlipStrings.Length];
		for (int j = 0; j < SlipStrings.Length; j++)
		{
			array2[j] = new DataCommand.CheckString
			{
				PrintText = new DataCommand.PrintString
				{
					Text = text2 + SlipStrings[j]
				}
			};
		}
		dataCommand.CheckStrings = array2;
		return dataCommand;
	}

	public string Translit(string s)
	{
		string text = "";
		string[] array = new string[73]
		{
			"А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И",
			"Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т",
			"У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь",
			"Э", "Ю", "Я", "а", "б", "в", "г", "д", "е", "ё",
			"ж", "з", "и", "й", "к", "л", "м", "н", "о", "п",
			"р", "с", "т", "у", "ф", "х", "ц", "ч", "ш", "щ",
			"ъ", "ы", "ь", "э", "ю", "я", ".", ",", "-", "+",
			":", ";", " "
		};
		string[] array2 = new string[73]
		{
			"A", "B", "V", "G", "D", "E", "E", "ZH", "Z", "I",
			"Y", "K", "L", "M", "N", "O", "P", "R", "S", "T",
			"U", "F", "KH", "TS", "CH", "SH", "SHCH", null, "Y", null,
			"E", "YU", "YA", "a", "b", "v", "g", "d", "e", "e",
			"zh", "z", "i", "y", "k", "l", "m", "n", "o", "p",
			"r", "s", "t", "u", "f", "kh", "ts", "ch", "sh", "shch",
			null, "y", null, "e", "yu", "ya", ".", ",", "-", "+",
			":", ";", " "
		};
		for (int i = 0; i < s.Length; i++)
		{
			bool flag = false;
			string text2 = s.Substring(i, 1);
			for (int j = 0; j < array.Length; j++)
			{
				if (text2 == array[j])
				{
					text += array2[j];
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				text += text2;
			}
		}
		return text;
	}

	public Dictionary<string, string> ReadIniFile(string FileName, Encoding Encoding)
	{
		string[] array = File.ReadAllText(FileName, Encoding).Replace("\r\n", "\r").Replace("\n", "\r")
			.Split('\r');
		int num = 0;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string text = array2[i].Trim();
			try
			{
				if (text == "" || text[0] == '#' || text[0] == ';')
				{
					dictionary.Add("KeyNull" + num++, text);
					continue;
				}
				string[] array3 = text.Split('=');
				if (array3.Length == 1)
				{
					dictionary.Add("KeyNull" + num++, array3[0]);
				}
				else if (array3.Length == 2)
				{
					dictionary.Add(array3[0].Trim(), array3[1].Trim());
				}
			}
			catch
			{
			}
		}
		return dictionary;
	}

	public void WriteIniFile(string FileName, Dictionary<string, string> Data, Encoding Encoding)
	{
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		foreach (KeyValuePair<string, string> Datum in Data)
		{
			if (!flag)
			{
				flag = true;
			}
			else
			{
				stringBuilder.Append("\r\n");
			}
			if (Datum.Key.IndexOf("KeyNull") != -1)
			{
				stringBuilder.Append(Datum.Value);
				continue;
			}
			stringBuilder.Append(Datum.Key);
			stringBuilder.Append("=");
			stringBuilder.Append(Datum.Value);
		}
		File.WriteAllText(FileName, stringBuilder.ToString(), Encoding);
	}

	public string GetValueForKeyUpp(Dictionary<string, string> Data, string Key)
	{
		foreach (KeyValuePair<string, string> Datum in Data)
		{
			if (Datum.Key.ToUpper() == Key.ToUpper())
			{
				return Datum.Value;
			}
		}
		return null;
	}

	public void SetValueForKeyUpp(Dictionary<string, string> Data, string Key, string Value, bool Rem = false)
	{
		foreach (KeyValuePair<string, string> Datum in Data)
		{
			if (Datum.Key.ToUpper() == Key.ToUpper())
			{
				if (!Rem)
				{
					Data[Datum.Key] = Value;
					return;
				}
				Data.Remove(Datum.Key);
				break;
			}
			if (Datum.Value.ToUpper().Trim() == (";" + Key + "=" + Value).ToUpper().Trim())
			{
				if (Rem)
				{
					Data[Datum.Key] = Value;
					return;
				}
				Data.Remove(Datum.Key);
				break;
			}
		}
		Data.Add((Rem ? ";" : "") + Key, Value);
	}

	private bool ProcessRoute(DataCommand DataCommand, RezultCommand RezultCommand, RezultCommandCheck RezultCommandCheck)
	{
		if (!SettDr.Paramets.ContainsKey("EmulationCheck") || !SettDr.Paramets["EmulationCheck"].AsBool() || !SettDr.Paramets.ContainsKey("RouteCommand") || SettDr.Paramets["RouteCommand"] == "")
		{
			return false;
		}
		if (DataCommand.Command == "RegisterCheck" && !DataCommand.IsFiscalCheck)
		{
			return false;
		}
		string idCommand = Guid.NewGuid().ToString();
		string idCommand2 = DataCommand.IdCommand;
		DataCommand.IdCommand = idCommand;
		string Rezult = "";
		HttpReqest(SettDr.Paramets["RouteCommand"], DataCommand, ref Rezult);
		ExecuteStatus executeStatus = ExecuteStatus.Error;
		if (RezultCommand != null)
		{
			RezultCommand source = null;
			if (RezultCommand is RezultCommandKKm)
			{
				source = JsonConvert.DeserializeObject<RezultCommandKKm>(Rezult, new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
			}
			else if (RezultCommand is RezultMarkingCodeValidation)
			{
				source = JsonConvert.DeserializeObject<RezultMarkingCodeValidation>(Rezult, new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
			}
			CopyObject(source, RezultCommand, "RezultProcessing");
			executeStatus = RezultCommand.Status;
		}
		else if (RezultCommandCheck != null)
		{
			CopyObject(JsonConvert.DeserializeObject<RezultCommandCheck>(Rezult, new JsonSerializerSettings
			{
				StringEscapeHandling = StringEscapeHandling.EscapeHtml
			}), RezultCommandCheck, "RezultProcessing");
			executeStatus = RezultCommandCheck.Status;
		}
		DataCommand.IdCommand = idCommand2;
		while (executeStatus == ExecuteStatus.NotRun || executeStatus == ExecuteStatus.Run)
		{
			for (int i = 0; i < 10; i++)
			{
				DataCommand dataCommand = new DataCommand();
				dataCommand.Command = "GetRezult";
				dataCommand.IdCommand = idCommand2;
				Rezult = "";
				HttpReqest(SettDr.Paramets["RouteCommand"], dataCommand, ref Rezult);
				RezultCommandGetRezult rezultCommandGetRezult = JsonConvert.DeserializeObject<RezultCommandGetRezult>(Rezult, new JsonSerializerSettings
				{
					StringEscapeHandling = StringEscapeHandling.EscapeHtml
				});
				if (RezultCommand != null)
				{
					CopyObject(RezultCommand, rezultCommandGetRezult.Rezult, "RezultProcessing");
					executeStatus = RezultCommand.Status;
				}
				else if (RezultCommandCheck != null)
				{
					CopyObject(RezultCommandCheck, rezultCommandGetRezult.Rezult, "RezultProcessing");
					executeStatus = RezultCommandCheck.Status;
				}
				if (executeStatus != ExecuteStatus.NotRun && executeStatus != ExecuteStatus.Run)
				{
					break;
				}
			}
		}
		if (RezultCommand != null)
		{
			RezultCommand.IdCommand = idCommand2;
			Error = RezultCommand.Error;
			if (RezultCommand.Status != ExecuteStatus.Ok)
			{
				throw new Exception("Выполняем команду удаленно: Ошибка: " + RezultCommand.Error);
			}
		}
		else if (RezultCommandCheck != null)
		{
			RezultCommandCheck.IdCommand = idCommand2;
			Error = RezultCommandCheck.Error;
			if (RezultCommandCheck.Status != ExecuteStatus.Ok)
			{
				throw new Exception("Выполняем команду удаленно: Ошибка: " + RezultCommand.Error);
			}
		}
		return true;
	}

	private void HttpReqest(string RouteURL, DataCommand DataCommand, ref string Rezult)
	{
		bool flag = false;
		string text = "";
		string text2 = "";
		string text3 = "";
		string text4 = "";
		string text5 = "";
		try
		{
			string text6 = RouteURL;
			flag = text6.ToLower().IndexOf("https") != -1;
			text6 = text6.Substring(text6.IndexOf("//") + 2);
			string text7 = text6.Substring(0, text6.IndexOf("@"));
			text = text7.Substring(0, text7.IndexOf(":"));
			text2 = text7.Substring(text7.IndexOf(":") + 1);
			text6 = text6.Substring(text6.IndexOf("@") + 1);
			text3 = text6.Substring(0, text6.IndexOf(":"));
			text6 = text6.Substring(text6.IndexOf(":") + 1);
			text4 = text6.Substring(0, text6.IndexOf("/"));
			text5 = text6.Substring(text6.IndexOf("/") + 1);
		}
		catch
		{
			throw new Exception("Ошибка забора строки URL маршрутизации");
		}
		bool value = DataCommand.NotPrint.Value;
		int numDevice = DataCommand.NumDevice;
		bool? payByProcessing = DataCommand.PayByProcessing;
		int numberCopies = DataCommand.NumberCopies;
		DataCommand.NotPrint = true;
		DataCommand.NumDevice = int.Parse(text5);
		DataCommand.PayByProcessing = false;
		DataCommand.NumberCopies = 0;
		string text8 = JsonConvert.SerializeObject(DataCommand, new JsonSerializerSettings
		{
			StringEscapeHandling = StringEscapeHandling.EscapeHtml
		});
		DataCommand.NotPrint = value;
		DataCommand.NumDevice = numDevice;
		DataCommand.PayByProcessing = payByProcessing;
		DataCommand.NumberCopies = numberCopies;
		byte[] bytes = Encoding.GetEncoding("UTF-8").GetBytes(text + ":" + text2);
		string text9 = null;
		Task<HttpResponseMessage> task = null;
		Task<string> task2 = null;
		try
		{
			string text10 = null;
			text10 = (flag ? ("https://" + text3 + ":" + text4 + "/Execute") : ("http://" + text3 + ":" + text4 + "/Execute"));
			HttpClient httpClient = new HttpClient(new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (HttpRequestMessage Request, X509Certificate2? certificate, X509Chain? chain, SslPolicyErrors sslPolicyErrors) => true
			});
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "Basic " + Convert.ToBase64String(bytes));
			httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
			httpClient.Timeout = TimeSpan.FromMilliseconds(35000.0);
			if (DataCommand.Timeout > 1)
			{
				httpClient.Timeout = TimeSpan.FromMilliseconds((DataCommand.Timeout + 10) * 1000);
			}
			task = httpClient.PostAsync(text10, new StringContent(text8, Encoding.UTF8, "application/json"));
			task.Wait();
		}
		catch (Exception ex)
		{
			text9 = Global.GetInnerErrorMessagee(ex.InnerException);
			try
			{
				task2 = task.Result.Content.ReadAsStringAsync();
				task2.Wait();
				text9 = text9 + "<br/>" + task2.Result;
			}
			catch
			{
			}
			Global.ErrorLicense = text9;
			throw new Exception("Выполняем команду удаленно: Ошибка. :" + text9);
		}
		task2 = task.Result.Content.ReadAsStringAsync();
		task2.Wait();
		PortLogs.Append("Выполняем команду удаленно: выполнено", "-");
		if (task.Result.StatusCode == HttpStatusCode.OK)
		{
			PortLogs.Append("Выполняем команду удаленно: выполнено успешно", "-");
			Rezult = task2.Result;
		}
	}

	public static void CopyObject(object Source, object Destination, string WithOut = "")
	{
		if (Source == null)
		{
			return;
		}
		string[] array = WithOut.Split(',');
		Type type = Destination.GetType();
		PropertyInfo[] properties = Source.GetType().GetProperties(BindingFlags.Public);
		foreach (PropertyInfo propertyInfo in properties)
		{
			bool flag = false;
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (propertyInfo.Name == text)
				{
					flag = true;
					break;
				}
			}
			if (!flag && propertyInfo.CanRead)
			{
				type.GetProperty(propertyInfo.Name, BindingFlags.Public).SetValue(Destination, propertyInfo.GetValue(Source));
			}
		}
		FieldInfo[] fields = Source.GetType().GetFields();
		foreach (FieldInfo fieldInfo in fields)
		{
			bool flag2 = false;
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				if (fieldInfo.Name == text2)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				type.GetField(fieldInfo.Name).SetValue(Destination, fieldInfo.GetValue(Source));
			}
		}
	}

	private async Task<RezultCommand> WaitWorkCommand(RezultCommand RezultCommand)
	{
		while (!RezultCommand.CommandEnd)
		{
			await Task.Delay(1000);
			try
			{
				await Global.UnitManager.ExecuteDatas.Semaphore.WaitAsync();
				foreach (UnitManager.ExecuteData executeData in Global.UnitManager.ExecuteDatas)
				{
					if (executeData.IdCommand == RezultCommand.IdCommand)
					{
						RezultCommand = (RezultCommandProcessing)executeData.RezultCommand;
						break;
					}
				}
			}
			catch (Exception ex)
			{
				RezultCommand.Status = ExecuteStatus.Error;
				RezultCommand.Error = "Не выполнена транзакция по оплате: " + ex.Message;
			}
			finally
			{
				Global.UnitManager.ExecuteDatas.Semaphore.Release();
			}
		}
		return RezultCommand;
	}

	public static void WindowTrackingStatus(DataCommand DataCommand, Unit Unit, string TextStatus, string TextQR = null, string PngBase64 = null)
	{
		UnitManager.ExecuteData executeData = null;
		bool flag = false;
		try
		{
			flag = Global.UnitManager.ExecuteDatas.Semaphore.Wait(1000);
			foreach (UnitManager.ExecuteData executeData2 in Global.UnitManager.ExecuteDatas)
			{
				if (executeData2.IdCommand == DataCommand.IdCommand && !executeData2.NotRelevant)
				{
					executeData = executeData2;
					break;
				}
			}
		}
		finally
		{
			if (flag)
			{
				Global.UnitManager.ExecuteDatas.Semaphore.Release();
			}
		}
		if (executeData != null && executeData.DataCommand != null && executeData.DataCommand.RunAsAddIn && !(executeData.CurrentNameCommand == ""))
		{
			if (DialogTrackingStatusHtml == null)
			{
				DialogTrackingStatusHtml = File.ReadAllText(Path.Combine(Global.GetPaht(), "html/DialogTrackingStatus.html"), Encoding.UTF8);
			}
			StringBuilder stringBuilder = new StringBuilder(DialogTrackingStatusHtml);
			stringBuilder.Replace("_Device_", TypeDevice.NameType[(int)executeData.Type]);
			if (Unit == null)
			{
				stringBuilder.Replace("_Model_", "Ожидание");
			}
			else
			{
				stringBuilder.Replace("_Model_", Unit.NameDevice);
			}
			stringBuilder.Replace("_Command_", executeData.CurrentNameCommand);
			stringBuilder.Replace("_Statys_", TextStatus);
			string NameOperation = "";
			decimal Summ = default(decimal);
			string CommandForAddOperation = "";
			UnitManager.GetInfoCommand(DataCommand, out NameOperation, out Summ, out CommandForAddOperation);
			if (Unit == null)
			{
				stringBuilder.Replace("_ОтменитьОперацию_", (NameOperation != "") ? (NameOperation + "..") : "Операция выполняется...");
				stringBuilder.Replace("_NumDevice_", "0");
			}
			else if (Unit != null && !Unit.IsCommandCancelled && (Unit.SettDr.TypeDevice.Type == TypeDevice.enType.ФискальныйРегистратор || Unit.SettDr.TypeDevice.Type == TypeDevice.enType.ПринтерЧеков))
			{
				stringBuilder.Replace("_NumDevice_", Unit.NumUnit.ToString());
				stringBuilder.Replace("_ОтменитьОперацию_", (NameOperation != "") ? ("Регистрация: " + NameOperation + "..") : "Регистрация чека...");
			}
			else if (Unit != null && !Unit.IsCommandCancelled && Unit.SettDr.TypeDevice.Type == TypeDevice.enType.ЭквайринговыйТерминал)
			{
				stringBuilder.Replace("_NumDevice_", Unit.NumUnit.ToString());
				stringBuilder.Replace("_ОтменитьОперацию_", "Для отмены нажмите кнопку на терминале");
			}
			else if (Unit != null && !Unit.IsCommandCancelled)
			{
				stringBuilder.Replace("_NumDevice_", Unit.NumUnit.ToString());
				stringBuilder.Replace("_ОтменитьОперацию_", (NameOperation != "") ? (NameOperation + "..") : "Операция выполняется...");
			}
			else if (Unit != null && Unit.IsCommandCancelled && !Unit.CancellationCommand)
			{
				stringBuilder.Replace("_NumDevice_", Unit.NumUnit.ToString());
				stringBuilder.Replace("_ОтменитьОперацию_", "Отменить операцию");
			}
			else if (Unit != null && Unit.IsCommandCancelled && Unit.CancellationCommand)
			{
				stringBuilder.Replace("_NumDevice_", "0");
				stringBuilder.Replace("_ОтменитьОперацию_", "Отменяем операцию...");
			}
			else
			{
				stringBuilder.Replace("_NumDevice_", "0");
				stringBuilder.Replace("_ОтменитьОперацию_", "");
			}
			if (TextQR != null)
			{
				stringBuilder.Replace("_TextQR_", TextQR);
			}
			if (PngBase64 != null)
			{
				stringBuilder.Replace("_QRbase64_", PngBase64);
			}
			else
			{
				stringBuilder.Replace("_QRbase64_", "iVBORw0KGgoAAAANSUhEUgAAAAIAAAACCAIAAAD91JpzAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYcAAB2HAY/l8WUAAAAPSURBVBhXY/gPBmDq/38AU7oL9YH+5D0AAAAASUVORK5CYII=");
			}
			if (TextQR != null || PngBase64 != null)
			{
				stringBuilder.Replace("_DisplayQR_", "block");
			}
			else
			{
				stringBuilder.Replace("_DisplayQR_", "none");
			}
			if (executeData.RezultCommand.MessageHTML == null || !(executeData.RezultCommand.TypeMessageHTM != "TrackingStatus"))
			{
				executeData.RezultCommand.TypeMessageHTM = "TrackingStatus";
				executeData.RezultCommand.MessageHTML = stringBuilder.ToString();
			}
		}
	}

	public static void HtmlDialogSelectDevice(DataCommand DataCommand, RezultCommand RezultCommand, RezultCommandUseAddInDialog RezultCommandUseAddInDialog, List<Unit> ListUnits, TypeDevice.enType Type)
	{
		if (DialogSelectDeviceHtml == null)
		{
			DialogSelectDeviceHtml = File.ReadAllText(Path.Combine(Global.GetPaht(), "html/DialogSelectDevice.html"), Encoding.UTF8);
		}
		StringBuilder stringBuilder = new StringBuilder(DialogSelectDeviceHtml);
		if (DialogSelectDeviceJs == null)
		{
			DialogSelectDeviceJs = File.ReadAllText(Path.Combine(Global.GetPaht(), "html/DialogSelectDevice.js"), Encoding.UTF8);
		}
		StringBuilder stringBuilder2 = new StringBuilder(DialogSelectDeviceJs);
		if (ListUnits.Count > 0)
		{
			foreach (Unit ListUnit in ListUnits)
			{
				string text = ListUnit.NumUnit + ": ";
				text = ((!(ListUnit.UnitName != "")) ? (text + ListUnit.NameDevice) : (text + ListUnit.UnitName));
				text = "<option value='" + ListUnit.NumUnit + "'>" + text + "</option>";
				stringBuilder.Replace("_СписокУстройств_", text + "_СписокУстройств_");
			}
			stringBuilder2.Replace("_SelectDevice_", ListUnits[0].NumUnit.ToString());
		}
		else
		{
			stringBuilder.Replace("_СписокУстройств_", "<option value=\"0\">Не выбрано</option>_СписокУстройств_");
			stringBuilder2.Replace("_SelectDevice_", "0");
		}
		stringBuilder.Replace("_СписокУстройств_", "");
		switch (Type)
		{
		case TypeDevice.enType.ФискальныйРегистратор:
			stringBuilder.Replace("_ВыполнитьОперацию_", "Зарегистрировать чек");
			break;
		case TypeDevice.enType.ЭквайринговыйТерминал:
			stringBuilder.Replace("_ВыполнитьОперацию_", "Выполнить транзакцию");
			break;
		default:
			stringBuilder.Replace("_ВыполнитьОперацию_", "Выполнить операцию");
			break;
		}
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
		string text2 = JsonConvert.SerializeObject(DataCommand, jsonSerializerSettings);
		stringBuilder2.Replace("_DataCommand_", text2);
		string text3 = JsonConvert.SerializeObject(RezultCommand, jsonSerializerSettings);
		stringBuilder2.Replace("_RezultCommand_", text3);
		stringBuilder.Replace("-UseAddInDialogscript-", stringBuilder2.ToString());
		stringBuilder.Replace("-UseAddInDialogId-", DataCommand.IdCommand);
		RezultCommandUseAddInDialog.UseAddInDialogHTML = stringBuilder.ToString();
		RezultCommandUseAddInDialog.UseAddInDialogScript = stringBuilder2.ToString();
	}

	public static void HtmlDialogPrintCheck(DataCommand DataCommand, RezultCommand RezultCommand, RezultCommandUseAddInDialog RezultCommandUseAddInDialog, List<Unit> ListUnits, TypeDevice.enType Type)
	{
		if (DialogPrintCheckHtml == null)
		{
			DialogPrintCheckHtml = File.ReadAllText(Path.Combine(Global.GetPaht(), "html/DialogPrintCheck.html"), Encoding.UTF8);
		}
		StringBuilder stringBuilder = new StringBuilder(DialogPrintCheckHtml);
		if (DialogPrintCheckJs == null)
		{
			DialogPrintCheckJs = File.ReadAllText(Path.Combine(Global.GetPaht(), "html/DialogPrintCheck.js"), Encoding.UTF8);
		}
		StringBuilder stringBuilder2 = new StringBuilder(DialogPrintCheckJs);
		List<Unit> ListSortUnits = Global.UnitManager.Units.Select(delegate(KeyValuePair<int, Unit> u)
		{
			KeyValuePair<int, Unit> keyValuePair = u;
			return keyValuePair.Value;
		}).ToList();
		List<Unit> list = new List<Unit>();
		foreach (Unit ListUnit in ListUnits)
		{
			if (!DataCommand.NumDeviceByProcessing.HasValue || DataCommand.NumDeviceByProcessing == 0)
			{
				int? num = int.Parse(ListUnit.SettDr.Paramets["NumDeviceByProcessing"]);
				if (num.HasValue && num != 0 && !list.Contains(Global.UnitManager.Units[num.Value]))
				{
					list.Add(Global.UnitManager.Units[num.Value]);
				}
			}
			else
			{
				try
				{
					list.Add(Global.UnitManager.Units[DataCommand.NumDeviceByProcessing.Value]);
				}
				catch
				{
				}
			}
		}
		foreach (Unit ListUnit2 in ListUnits)
		{
			new DataCommand
			{
				Command = "PayByPaymentCard",
				InnKkm = ListUnit2.Kkm.INN,
				TaxVariant = DataCommand.TaxVariant
			};
			foreach (Unit item in UnitManager.GetListUnitsForCommand(DataCommand, TypeDevice.enType.ЭквайринговыйТерминал, ref ListSortUnits))
			{
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		if (list.Count > 0)
		{
			foreach (Unit item2 in list)
			{
				string text = item2.NumUnit + ": ";
				text = ((!(item2.UnitName != "")) ? (text + item2.NameDevice) : (text + item2.UnitName));
				text = "<option value='" + item2.NumUnit + "'>" + text + "</option>";
				stringBuilder.Replace("_СписокТерминалов_", text + "_СписокТерминалов_");
			}
			stringBuilder2.Replace("_SelectTerminal_", list[0].NumUnit.ToString());
		}
		else
		{
			stringBuilder.Replace("_СписокТерминалов_", "<option value=\"0\">Не выбрано</option>_СписокТерминалов_");
			stringBuilder2.Replace("_SelectTerminal_", "0");
		}
		stringBuilder.Replace("_СписокТерминалов_", "");
		if (ListUnits.Count > 0)
		{
			foreach (Unit ListUnit3 in ListUnits)
			{
				string text2 = ListUnit3.NumUnit + ": ";
				text2 = ((!(ListUnit3.UnitName != "")) ? (text2 + ListUnit3.NameDevice) : (text2 + ListUnit3.UnitName));
				text2 = "<option value='" + ListUnit3.NumUnit + "'>" + text2 + "</option>";
				stringBuilder.Replace("_СписокУстройств_", text2 + "_СписокУстройств_");
			}
			stringBuilder2.Replace("_SelectDevice_", ListUnits[0].NumUnit.ToString());
		}
		else
		{
			stringBuilder.Replace("_СписокУстройств_", "<option value=\"0\">Не выбрано</option>_СписокУстройств_");
			stringBuilder2.Replace("_SelectDevice_", "0");
		}
		stringBuilder.Replace("_СписокУстройств_", "");
		decimal num2 = default(decimal);
		if (DataCommand.CheckStrings != null)
		{
			DataCommand.CheckString[] checkStrings = DataCommand.CheckStrings;
			foreach (DataCommand.CheckString checkString in checkStrings)
			{
				if (DataCommand.IsFiscalCheck && checkString != null && checkString.Register != null)
				{
					num2 += checkString.Register.Amount;
				}
			}
		}
		stringBuilder2.Replace("_Cash_", DataCommand.Cash.ToString().Replace(',', '.'));
		stringBuilder2.Replace("_ElectronicPayment_", "0");
		stringBuilder2.Replace("_Card_", DataCommand.ElectronicPayment.ToString().Replace(',', '.'));
		stringBuilder2.Replace("_AdvancePayment_", DataCommand.AdvancePayment.ToString().Replace(',', '.'));
		stringBuilder2.Replace("_Credit_", DataCommand.Credit.ToString().Replace(',', '.'));
		stringBuilder2.Replace("_CashCashProvision_", DataCommand.CashProvision.ToString().Replace(',', '.'));
		stringBuilder2.Replace("_Result_", (DataCommand.Cash + DataCommand.ElectronicPayment + DataCommand.AdvancePayment + DataCommand.Credit + DataCommand.CashProvision).ToString().Replace(',', '.'));
		stringBuilder2.Replace("_Ammount_", num2.ToString().Replace(',', '.'));
		stringBuilder.Replace("_Cash_", DataCommand.Cash.ToString().Replace(',', '.'));
		stringBuilder.Replace("_ElectronicPayment_", "0");
		stringBuilder.Replace("_Card_", DataCommand.ElectronicPayment.ToString().Replace(',', '.'));
		stringBuilder.Replace("_AdvancePayment_", DataCommand.AdvancePayment.ToString().Replace(',', '.'));
		stringBuilder.Replace("_Credit_", DataCommand.Credit.ToString().Replace(',', '.'));
		stringBuilder.Replace("_CashCashProvision_", DataCommand.CashProvision.ToString().Replace(',', '.'));
		stringBuilder.Replace("_Result_", (DataCommand.Cash + DataCommand.ElectronicPayment + DataCommand.AdvancePayment + DataCommand.Credit + DataCommand.CashProvision).ToString().Replace(',', '.'));
		JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
		string text3 = JsonConvert.SerializeObject(DataCommand, jsonSerializerSettings);
		stringBuilder2.Replace("_DataCommand_", text3);
		string text4 = JsonConvert.SerializeObject(RezultCommand, jsonSerializerSettings);
		stringBuilder2.Replace("_RezultCommand_", text4);
		stringBuilder.Replace("-UseAddInDialogscript-", stringBuilder2.ToString());
		stringBuilder.Replace("-UseAddInDialogId-", DataCommand.IdCommand);
		RezultCommandUseAddInDialog.UseAddInDialogHTML = stringBuilder.ToString();
		RezultCommandUseAddInDialog.UseAddInDialogScript = stringBuilder2.ToString();
	}
}
