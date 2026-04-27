using System;
using Atol.Drivers10.Fptr;

namespace KkmFactory;

internal class Fptr
{
	private Atol.Drivers10.Fptr.Fptr Atol10;

	private Unit Unit;

	public Fptr(Unit Unit)
	{
		this.Unit = Unit;
		Atol10 = new Atol.Drivers10.Fptr.Fptr("KKT" + Unit.NumUnit, null);
	}

	public void Close()
	{
		try
		{
			Atol10.close();
		}
		catch
		{
		}
	}

	public void FreeLibrary()
	{
		try
		{
			Atol10.close();
		}
		catch
		{
		}
	}

	public string version()
	{
		string text = Atol10.version();
		Unit.PortLogs.Append("version() = " + text.ToString(), "<");
		return text;
	}

	public int logWrite(string tag, int level, string message)
	{
		return Atol10.logWrite(tag, level, message);
	}

	public int showProperties(int parentType, nint parent)
	{
		int result = Atol10.showProperties(parentType, parent);
		Unit.PortLogs.Append("showProperties(" + parentType + ", " + (IntPtr)parent + ") = " + result, "<");
		return result;
	}

	public bool isOpened()
	{
		if (Atol10 != null)
		{
			bool result = Atol10.isOpened();
			Unit.PortLogs.Append("isOpened() = " + result, "<");
			return result;
		}
		return false;
	}

	public int errorCode()
	{
		int result = Atol10.errorCode();
		Unit.PortLogs.Append("errorCode() = " + result, "<");
		return result;
	}

	public string errorDescription()
	{
		string text = Atol10.errorDescription();
		Unit.PortLogs.Append("errorDescription() = " + text.ToString(), "<");
		return text;
	}

	public void resetError()
	{
		Atol10.resetError();
		Unit.PortLogs.Append("resetError()", "<");
	}

	public int setSettings(string settings)
	{
		int result = Atol10.setSettings(settings);
		Unit.PortLogs.Append("setSettings(" + settings + ") = " + result, "<");
		return result;
	}

	public string getSettings()
	{
		string settings = Atol10.getSettings();
		Unit.PortLogs.Append("getSettings() = " + settings.ToString());
		return settings;
	}

	public void setSingleSetting(string key, string setting)
	{
		Atol10.setSingleSetting(key, setting);
		Unit.PortLogs.Append("setSingleSetting(" + key.ToString() + ", " + setting.ToString() + ")", "<");
	}

	public string getSingleSetting(string key)
	{
		string singleSetting = Atol10.getSingleSetting(key);
		Unit.PortLogs.Append("getSingleSetting(" + key + ") = " + singleSetting.ToString());
		return singleSetting;
	}

	public void setParam(int paramID, uint value)
	{
		Atol10.setParam(paramID, value);
		Unit.PortLogs.Append("setParam(" + paramID + ", " + value + ")", "<");
	}

	public void setParam(int paramID, bool value)
	{
		Atol10.setParam(paramID, value);
		Unit.PortLogs.Append("setParam(" + paramID + ", " + value + ")", "<");
	}

	public void setParam(int paramID, double value)
	{
		Atol10.setParam(paramID, value);
		Unit.PortLogs.Append("setParam(" + paramID + ", " + value + ")", "<");
	}

	public void setParam(int paramID, byte[] value)
	{
		Atol10.setParam(paramID, value);
		Unit.PortLogs.Append("setParam(" + paramID + ", " + value.ToString() + ")", "<");
	}

	public void setParam(int paramID, DateTime value)
	{
		Atol10.setParam(paramID, value);
		Unit.PortLogs.Append("setParam(" + paramID + ", " + value.ToString() + ")", "<");
	}

	public void setParam(int paramID, string value)
	{
		Atol10.setParam(paramID, value);
		Unit.PortLogs.Append("setParam(" + paramID + ", " + value.ToString() + ")", "<");
	}

	public void setNonPrintableParam(int paramID, uint value)
	{
		Atol10.setNonPrintableParam(paramID, value);
		Unit.PortLogs.Append("setNonPrintableParam(" + paramID + ", " + value + ")", "<");
	}

	public void setNonPrintableParam(int paramID, bool value)
	{
		Atol10.setNonPrintableParam(paramID, value);
		Unit.PortLogs.Append("setNonPrintableParam(" + paramID + ", " + value + ")", "<");
	}

	public void setNonPrintableParam(int paramID, double value)
	{
		Atol10.setNonPrintableParam(paramID, value);
		Unit.PortLogs.Append("setNonPrintableParam(" + paramID + ", " + value + ")", "<");
	}

	public void setNonPrintableParam(int paramID, byte[] value)
	{
		Atol10.setNonPrintableParam(paramID, value);
		Unit.PortLogs.Append("setNonPrintableParam(" + paramID + ", " + value.ToString() + ")", "<");
	}

	public void setNonPrintableParam(int paramID, DateTime value)
	{
		Atol10.setNonPrintableParam(paramID, value);
		Unit.PortLogs.Append("setNonPrintableParam(" + paramID + ", " + value.ToString() + ")", "<");
	}

	public void setNonPrintableParam(int paramID, string value)
	{
		Atol10.setNonPrintableParam(paramID, value);
		Unit.PortLogs.Append("setNonPrintableParam(" + paramID + ", " + value.ToString() + ")", "<");
	}

	public void setUserParam(int paramID, uint value)
	{
		Atol10.setUserParam(paramID, value);
		Unit.PortLogs.Append("setUserParam(" + paramID + ", " + value + ")", "<");
	}

	public void setUserParam(int paramID, bool value)
	{
		Atol10.setUserParam(paramID, value);
		Unit.PortLogs.Append("setUserParam(" + paramID + ", " + value + ")", "<");
	}

	public void setUserParam(int paramID, double value)
	{
		Atol10.setUserParam(paramID, value);
		Unit.PortLogs.Append("setUserParam(" + paramID + ", " + value + ")", "<");
	}

	public void setUserParam(int paramID, byte[] value)
	{
		Atol10.setUserParam(paramID, value);
		Unit.PortLogs.Append("setUserParam(" + paramID + ", " + value.ToString() + ")", "<");
	}

	public void setUserParam(int paramID, DateTime value)
	{
		Atol10.setUserParam(paramID, value);
		Unit.PortLogs.Append("setUserParam(" + paramID + ", " + value.ToString() + ")", "<");
	}

	public void setUserParam(int paramID, string value)
	{
		Atol10.setUserParam(paramID, value);
		Unit.PortLogs.Append("setUserParam(" + paramID + ", " + value.ToString() + ")", "<");
	}

	public uint getParamInt(int paramID)
	{
		uint paramInt = Atol10.getParamInt(paramID);
		Unit.PortLogs.Append("getParamInt(" + paramID + ") = " + paramInt);
		return paramInt;
	}

	public bool getParamBool(int paramID)
	{
		bool paramBool = Atol10.getParamBool(paramID);
		Unit.PortLogs.Append("getParamBool(" + paramID + ") = " + paramBool);
		return paramBool;
	}

	public double getParamDouble(int paramID)
	{
		double paramDouble = Atol10.getParamDouble(paramID);
		Unit.PortLogs.Append("getParamDouble(" + paramID + ") = " + paramDouble);
		return paramDouble;
	}

	public byte[] getParamByteArray(int paramID)
	{
		byte[] paramByteArray = Atol10.getParamByteArray(paramID);
		Unit.PortLogs.Append("getParamByteArray(" + paramID + ") = " + paramByteArray.ToString());
		return paramByteArray;
	}

	public DateTime getParamDateTime(int paramID)
	{
		DateTime paramDateTime = Atol10.getParamDateTime(paramID);
		Unit.PortLogs.Append("getParamDateTime(" + paramID + ") = " + paramDateTime);
		return paramDateTime;
	}

	public string getParamString(int paramID)
	{
		string paramString = Atol10.getParamString(paramID);
		Unit.PortLogs.Append("getParamString(" + paramID + ") = " + paramString.ToString());
		return paramString;
	}

	public int applySingleSettings()
	{
		int result = Atol10.applySingleSettings();
		Unit.PortLogs.Append("applySingleSettings() = " + result, "<");
		return result;
	}

	public int open()
	{
		int result = Atol10.open();
		Unit.PortLogs.Append("open() = " + result, "<");
		return result;
	}

	public int close()
	{
		int result = Atol10.close();
		Unit.PortLogs.Append("close() = " + result, "<");
		return result;
	}

	public int resetParams()
	{
		int result = Atol10.resetParams();
		Unit.PortLogs.Append("resetParams() = " + result, "<");
		return result;
	}

	public int runCommand()
	{
		int result = Atol10.runCommand();
		Unit.PortLogs.Append("runCommand() = " + result, "<");
		return result;
	}

	public int beep()
	{
		int result = Atol10.beep();
		Unit.PortLogs.Append("beep() = " + result, "<");
		return result;
	}

	public int openDrawer()
	{
		int result = Atol10.openDrawer();
		Unit.PortLogs.Append("openDrawer() = " + result, "<");
		return result;
	}

	public int cut()
	{
		int result = Atol10.cut();
		Unit.PortLogs.Append("cut() = " + result, "<");
		return result;
	}

	public int devicePoweroff()
	{
		int result = Atol10.devicePoweroff();
		Unit.PortLogs.Append("devicePoweroff() = " + result, "<");
		return result;
	}

	public int deviceReboot()
	{
		int result = Atol10.deviceReboot();
		Unit.PortLogs.Append("deviceReboot() = " + result, "<");
		return result;
	}

	public int openShift()
	{
		int result = Atol10.openShift();
		Unit.PortLogs.Append("openShift() = " + result, "<");
		return result;
	}

	public int resetSummary()
	{
		int result = Atol10.resetSummary();
		Unit.PortLogs.Append("resetSummary() = " + result, "<");
		return result;
	}

	public int initDevice()
	{
		int result = Atol10.initDevice();
		Unit.PortLogs.Append("initDevice() = " + result, "<");
		return result;
	}

	public int queryData()
	{
		int result = Atol10.queryData();
		Unit.PortLogs.Append("queryData() = " + result, "<");
		return result;
	}

	public int cashIncome()
	{
		int result = Atol10.cashIncome();
		Unit.PortLogs.Append("cashIncome() = " + result, "<");
		return result;
	}

	public int cashOutcome()
	{
		int result = Atol10.cashOutcome();
		Unit.PortLogs.Append("cashOutcome() = " + result, "<");
		return result;
	}

	public int openReceipt()
	{
		int result = Atol10.openReceipt();
		Unit.PortLogs.Append("openReceipt() = " + result, "<");
		return result;
	}

	public int cancelReceipt()
	{
		int result = Atol10.cancelReceipt();
		Unit.PortLogs.Append("cancelReceipt() = " + result, "<");
		return result;
	}

	public int closeReceipt()
	{
		int result = Atol10.closeReceipt();
		Unit.PortLogs.Append("closeReceipt() = " + result, "<");
		return result;
	}

	public int checkDocumentClosed()
	{
		int result = Atol10.checkDocumentClosed();
		Unit.PortLogs.Append("checkDocumentClosed() = " + result, "<");
		return result;
	}

	public int receiptTotal()
	{
		int result = Atol10.receiptTotal();
		Unit.PortLogs.Append("receiptTotal() = " + result, "<");
		return result;
	}

	public int receiptTax()
	{
		int result = Atol10.receiptTax();
		Unit.PortLogs.Append("receiptTax() = " + result, "<");
		return result;
	}

	public int registration()
	{
		int result = Atol10.registration();
		Unit.PortLogs.Append("registration() = " + result, "<");
		return result;
	}

	public int payment()
	{
		int result = Atol10.payment();
		Unit.PortLogs.Append("libfptr_payment() = " + result, "<");
		return result;
	}

	public int report()
	{
		int result = Atol10.report();
		Unit.PortLogs.Append("report() = " + result, "<");
		return result;
	}

	public int printText()
	{
		int result = Atol10.printText();
		Unit.PortLogs.Append("printText() = " + result, "<");
		return result;
	}

	public int printCliche()
	{
		int result = Atol10.printCliche();
		Unit.PortLogs.Append("printCliche() = " + result, "<");
		return result;
	}

	public int beginNonfiscalDocument()
	{
		int result = Atol10.beginNonfiscalDocument();
		Unit.PortLogs.Append("beginNonfiscalDocument() = " + result, "<");
		return result;
	}

	public int endNonfiscalDocument()
	{
		int result = Atol10.endNonfiscalDocument();
		Unit.PortLogs.Append("endNonfiscalDocument() = " + result, "<");
		return result;
	}

	public int printBarcode()
	{
		int result = Atol10.printBarcode();
		Unit.PortLogs.Append("printBarcode() = " + result, "<");
		return result;
	}

	public int printPicture()
	{
		int result = Atol10.printPicture();
		Unit.PortLogs.Append("printPicture() = " + result, "<");
		return result;
	}

	public int printPictureByNumber()
	{
		int result = Atol10.printPictureByNumber();
		Unit.PortLogs.Append("printPictureByNumber() = " + result, "<");
		return result;
	}

	public int uploadPictureFromFile()
	{
		int result = Atol10.uploadPictureFromFile();
		Unit.PortLogs.Append("uploadPictureFromFile() = " + result, "<");
		return result;
	}

	public int clearPictures()
	{
		int result = Atol10.clearPictures();
		Unit.PortLogs.Append("clearPictures() = " + result, "<");
		return result;
	}

	public int writeDeviceSettingRaw()
	{
		int result = Atol10.writeDeviceSettingRaw();
		Unit.PortLogs.Append("writeDeviceSettingRaw() = " + result, "<");
		return result;
	}

	public int readDeviceSettingRaw()
	{
		int result = Atol10.readDeviceSettingRaw();
		Unit.PortLogs.Append("readDeviceSettingRaw() = " + result, "<");
		return result;
	}

	public int commitSettings()
	{
		int result = Atol10.commitSettings();
		Unit.PortLogs.Append("commitSettings() = " + result, "<");
		return result;
	}

	public int initSettings()
	{
		int result = Atol10.initSettings();
		Unit.PortLogs.Append("initSettings() = " + result, "<");
		return result;
	}

	public int resetSettings()
	{
		int result = Atol10.resetSettings();
		Unit.PortLogs.Append("resetSettings() = " + result, "<");
		return result;
	}

	public int writeDateTime()
	{
		int result = Atol10.writeDateTime();
		Unit.PortLogs.Append("writeDateTime() = " + result, "<");
		return result;
	}

	public int writeLicense()
	{
		int result = Atol10.writeLicense();
		Unit.PortLogs.Append("writeLicense() = " + result, "<");
		return result;
	}

	public int fnOperation()
	{
		int result = Atol10.fnOperation();
		Unit.PortLogs.Append("fnOperation() = " + result, "<");
		return result;
	}

	public int fnQueryData()
	{
		int result = Atol10.fnQueryData();
		Unit.PortLogs.Append("fnQueryData() = " + result, "<");
		return result;
	}

	public int fnWriteAttributes()
	{
		int result = Atol10.fnWriteAttributes();
		Unit.PortLogs.Append("fnWriteAttributes() = " + result, "<");
		return result;
	}

	public int externalDevicePowerOn()
	{
		int result = Atol10.externalDevicePowerOn();
		Unit.PortLogs.Append("externalDevicePowerOn() = " + result, "<");
		return result;
	}

	public int externalDevicePowerOff()
	{
		int result = Atol10.externalDevicePowerOff();
		Unit.PortLogs.Append("externalDevicePowerOff() = " + result, "<");
		return result;
	}

	public int externalDeviceWriteData()
	{
		int result = Atol10.externalDeviceWriteData();
		Unit.PortLogs.Append("externalDeviceWriteData() = " + result, "<");
		return result;
	}

	public int externalDeviceReadData()
	{
		int result = Atol10.externalDeviceReadData();
		Unit.PortLogs.Append("externalDeviceReadData() = " + result, "<");
		return result;
	}

	public int operatorLogin()
	{
		int result = Atol10.operatorLogin();
		Unit.PortLogs.Append("operatorLogin() = " + result, "<");
		return result;
	}

	public int processJson()
	{
		int result = Atol10.processJson();
		Unit.PortLogs.Append("processJson() = " + result, "<");
		return result;
	}

	public int readDeviceSetting()
	{
		int result = Atol10.readDeviceSetting();
		Unit.PortLogs.Append("readDeviceSetting() = " + result, "<");
		return result;
	}

	public int writeDeviceSetting()
	{
		int result = Atol10.writeDeviceSetting();
		Unit.PortLogs.Append("writeDeviceSetting() = " + result, "<");
		return result;
	}

	public int beginReadRecords()
	{
		int result = Atol10.beginReadRecords();
		Unit.PortLogs.Append("beginReadRecords() = " + result, "<");
		return result;
	}

	public int readNextRecord()
	{
		int result = Atol10.readNextRecord();
		Unit.PortLogs.Append("readNextRecord() = " + result, "<");
		return result;
	}

	public int endReadRecords()
	{
		int result = Atol10.endReadRecords();
		Unit.PortLogs.Append("endReadRecords() = " + result, "<");
		return result;
	}

	public int userMemoryOperation()
	{
		int result = Atol10.userMemoryOperation();
		Unit.PortLogs.Append("userMemoryOperation() = " + result, "<");
		return result;
	}

	public int continuePrint()
	{
		int result = Atol10.continuePrint();
		Unit.PortLogs.Append("continuePrint() = " + result, "<");
		return result;
	}

	public int initMgm()
	{
		int result = initMgm();
		Unit.PortLogs.Append("initMgm() = " + result, "<");
		return result;
	}

	public int utilFormTlv()
	{
		int result = Atol10.utilFormTlv();
		Unit.PortLogs.Append("utilFormTlv() = " + result, "<");
		return result;
	}

	public int utilFormNomenclature()
	{
		int result = Atol10.utilFormNomenclature();
		Unit.PortLogs.Append("utilFormNomenclature() = " + result, "<");
		return result;
	}

	public int utilMapping()
	{
		int result = Atol10.utilMapping();
		Unit.PortLogs.Append("utilMapping() = " + result, "<");
		return result;
	}

	public int readModelFlags()
	{
		int result = Atol10.readModelFlags();
		Unit.PortLogs.Append("readModelFlags() = " + result, "<");
		return result;
	}

	public int lineFeed()
	{
		int result = Atol10.lineFeed();
		Unit.PortLogs.Append("lineFeed() = " + result, "<");
		return result;
	}

	public int flashFirmware()
	{
		int result = Atol10.flashFirmware();
		Unit.PortLogs.Append("flashFirmware() = " + result, "<");
		return result;
	}

	public int softLockInit()
	{
		int result = Atol10.softLockInit();
		Unit.PortLogs.Append("softLockInit() = " + result, "<");
		return result;
	}

	public int softLockQuerySessionCode()
	{
		int result = Atol10.softLockQuerySessionCode();
		Unit.PortLogs.Append("softLockQuerySessionCode() = " + result, "<");
		return result;
	}

	public int softLockValidate()
	{
		int result = Atol10.softLockValidate();
		Unit.PortLogs.Append("softLockValidate() = " + result, "<");
		return result;
	}

	public int utilCalcTax()
	{
		int result = Atol10.utilCalcTax();
		Unit.PortLogs.Append("utilCalcTax() = " + result, "<");
		return result;
	}

	public int downloadPicture()
	{
		int result = Atol10.downloadPicture();
		Unit.PortLogs.Append("downloadPicture() = " + result, "<");
		return result;
	}

	public int bluetoothRemovePairedDevices()
	{
		int result = Atol10.bluetoothRemovePairedDevices();
		Unit.PortLogs.Append("bluetoothRemovePairedDevices() = " + result, "<");
		return result;
	}

	public int utilTagInfo()
	{
		int result = Atol10.utilTagInfo();
		Unit.PortLogs.Append("utilTagInfo() = " + result, "<");
		return result;
	}

	public int utilContainerVersions()
	{
		int result = Atol10.utilContainerVersions();
		Unit.PortLogs.Append("utilContainerVersions() = " + result, "<");
		return result;
	}

	public int activateLicenses()
	{
		int result = Atol10.activateLicenses();
		Unit.PortLogs.Append("activateLicenses() = " + result, "<");
		return result;
	}

	public int removeLicenses()
	{
		int result = Atol10.removeLicenses();
		Unit.PortLogs.Append("removeLicenses() = " + result, "<");
		return result;
	}

	public int enterKeys()
	{
		int result = Atol10.enterKeys();
		Unit.PortLogs.Append("enterKeys() = " + result, "<");
		return result;
	}

	public int validateKeys()
	{
		int result = Atol10.validateKeys();
		Unit.PortLogs.Append("validateKeys() = " + result, "<");
		return result;
	}

	public int enterSerialNumber()
	{
		int result = Atol10.enterSerialNumber();
		Unit.PortLogs.Append("enterSerialNumber() = " + result, "<");
		return result;
	}

	public int getSerialNumberRequest()
	{
		int serialNumberRequest = Atol10.getSerialNumberRequest();
		Unit.PortLogs.Append("getSerialNumberRequest() = " + serialNumberRequest, "<");
		return serialNumberRequest;
	}

	public int uploadPixelBuffer()
	{
		int result = Atol10.uploadPixelBuffer();
		Unit.PortLogs.Append("uploadPixelBuffer() = " + result, "<");
		return result;
	}

	public int downloadPixelBuffer()
	{
		int result = Atol10.downloadPixelBuffer();
		Unit.PortLogs.Append("downloadPixelBuffer() = " + result, "<");
		return result;
	}

	public int printPixelBuffer()
	{
		int result = Atol10.printPixelBuffer();
		Unit.PortLogs.Append("printPixelBuffer() = " + result, "<");
		return result;
	}

	public int utilConvertTagValue()
	{
		int result = Atol10.utilConvertTagValue();
		Unit.PortLogs.Append("utilConvertTagValue() = " + result, "<");
		return result;
	}

	public int parseMarkingCode()
	{
		int result = Atol10.parseMarkingCode();
		Unit.PortLogs.Append("parseMarkingCode() = " + result, "<");
		return result;
	}

	public int callScript()
	{
		int result = Atol10.callScript();
		Unit.PortLogs.Append("callScript() = " + result, "<");
		return result;
	}

	public int setHeaderLines()
	{
		int result = Atol10.setHeaderLines();
		Unit.PortLogs.Append("setHeaderLines() = " + result, "<");
		return result;
	}

	public int setFooterLines()
	{
		int result = Atol10.setFooterLines();
		Unit.PortLogs.Append("setFooterLines() = " + result, "<");
		return result;
	}

	public int beginMarkingCodeValidation()
	{
		int result = Atol10.beginMarkingCodeValidation();
		Unit.PortLogs.Append("beginMarkingCodeValidation() = " + result, "<");
		return result;
	}

	public int getMarkingCodeValidationStatus()
	{
		int markingCodeValidationStatus = Atol10.getMarkingCodeValidationStatus();
		Unit.PortLogs.Append("getMarkingCodeValidationStatus() = " + markingCodeValidationStatus, "<");
		return markingCodeValidationStatus;
	}

	public int acceptMarkingCode()
	{
		int result = Atol10.acceptMarkingCode();
		Unit.PortLogs.Append("acceptMarkingCode() = " + result, "<");
		return result;
	}

	public int declineMarkingCode()
	{
		int result = Atol10.declineMarkingCode();
		Unit.PortLogs.Append("declineMarkingCode() = " + result, "<");
		return result;
	}

	public int clearMarkingCodeValidationResult()
	{
		int result = Atol10.clearMarkingCodeValidationResult();
		Unit.PortLogs.Append("clearMarkingCodeValidationResult() = " + result, "<");
		return result;
	}

	public int cancelMarkingCodeValidation()
	{
		int result = Atol10.cancelMarkingCodeValidation();
		Unit.PortLogs.Append("cancelMarkingCodeValidation() = " + result, "<");
		return result;
	}

	public int updateFnmKeys()
	{
		int result = Atol10.updateFnmKeys();
		Unit.PortLogs.Append("updateFnmKeys() = " + result, "<");
		return result;
	}
}
