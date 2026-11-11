mergeInto(LibraryManager.library,
{
	LangRequest_js: function ()
	{
		var sdkLanguage = ysdk && ysdk.environment && ysdk.environment.i18n && ysdk.environment.i18n.lang;
		var returnStr = (sdkLanguage || navigator.language || "en").split("-")[0];
		var bufferSize = lengthBytesUTF8(returnStr) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(returnStr, buffer, bufferSize);
		return buffer;
	},
	
	GeneralLanguage_js: function ()
	{
		var returnStr = navigator.language || navigator.userLanguage || "en";
		var bufferSize = lengthBytesUTF8(returnStr) + 1;
		var buffer = _malloc(bufferSize);
		stringToUTF8(returnStr, buffer, bufferSize);
		return buffer;
	}
});