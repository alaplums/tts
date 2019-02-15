package net.sourceforge.texlipse.TTSIntegration;

public class TTSProperties {
	
	public static final String SEND_DATA_NEXT = "**(tNext)**";
	public static final String SEND_DATA_STOP = "**(tStop)**";
	public static final String SEND_DATA_LATEX_DCMNT = "**(LatexDocument)**";
	public static final String SEND_DATA_MATHMODE = "**(tMathMode)**";
	public static final String SEND_DATA_PAUSE = "**(tPAUSE)**";
	public static final String SEND_DATA_RESUME = "**(tRESUME)**";
	public static final String SEND_DATA_WORD_VRBSTY = "**(VBRSTYWord)**";
	public static final String SEND_DATA_CHAR_VRBSTY = "**(VBRSTYChar)**";
	public static final String SEND_DATA_SPECIAL_CMND = "**(tSpecialCommand)**";
	public static final String SEND_DATA_INCREASE_SPEED = "**(tIncreaseSpeed)**";
	public static final String SEND_DATA_DECREASE_SPEED = "**(tDecreaseSpeed)**";
	public static final String SEND_DATA_EDITOR_CMND = "**(TEditorCommand)**";
	public static final String SEND_DATA_CHANGE_VOICE = "**(changeVoice)**";
	public static final String SEND_DATA_PDFACESBLTY_MODE="**(PDF accessibility Mode)**";
	public static final String SEND_DATA_CREATE_PROJECT = "Latex Project";
	public static final String SEND_DATA_ENTER_PROJECT_NAME ="Enter Project Name";
	public static final String SEND_DATA_ISO_STANDARD ="The Two Letter (ISO 639 standard) language code e n";
	public static final String SEND_DATA_TWO_LETTER_STANDARD ="The two - letter (ISO 639 standard) language code e n";
	public static final String SEND_DATA_SELECTED_PROJECT = "Selected Create project in workspace";
	public static final String SEND_DATA_SELECTED_PROJ = "Selected Create project in workspace";
	public static final String SEND_DATA_FOCUS="Focus on setup build tools";
	public static final String SEND_DATA_EXTERNAL_LOCATION="Selected create project at external location";
	public static final String SEND_DATA_FOCUS_TOOL="Focus on setup build tools";
	public static final String SEND_DATA_BASIC_ARTICLE="This is very basic article template. \n there is just one selection and two subsections.";
	public static final String SEND_DATA_NEXT_="Next";
	public static final String SEND_DATA_CANCEL="Cancel";
	public static final String SEND_DATA_ENTER_PROJ_NAME="Enter Project Name";
	
	public static final String MSG_WARNING_ACESBLTY_FORM_LABELS = "Missing labels are not recomended as per PDF accessibility standards";
	public static final String MSG_WARNING_ACESBLTY_FORM_HIDDEN_FIELDS = "Hidden fields are not recomended as per PDF accessibility standards";
	public static final String MSG_WARNING_ACESBLTY_FORM_TOOLTIP = "Tool tip for every form field is recommended as per PDF accessibility standards";
	public static final String MSG_WARNING_ACESBLTY_COMENTS = "Comments are not recommended as per PDF accessiblity standards";
	public static final String MSG_WARNING_ACESBLTY_URL = "InValid URLs are not recommended as per PDF accessibility standards";
	public static final String MSG_WARNING_ACESBLTY_CUSTMCMNDS_TOOLTIP = "Tool tips for custom commands are recommended as per PDF accessibility standards";
	public static final String MSG_WARNING_ACESBLTY_LONG_TABLE = "If you want a row head then use a long table package";
	public static final String MSG_WARNING_ACESBLTY_BLANK_CELL = "Blank cell are not allowed as per PDF accessibility standards";
	public static final String MSG_WARNING_ACESBLTY_MISNG_ROWHEADER = "If you want a row head then after the first row use \\endhead tag";
	public static final String MSG_WARNING_ACESBLTY_CAPTION = "Caption should be provided with a figure as per PDF accessibility standards";
	public static final String MSG_WARNING_ACESBLTY_ALT = "Alternative text should be provided with a figure as per PDF accessibility standards";
	public static final String MSG_WARNING_ACESBLTY_FONT = "The Provided Font Style are not recommended as per PDF accessibility standards";
	public static final String MSG_WARNING_ACESBLTY_TRCK_CHNGS = "Track changes are not recommended as per PDF accessibility standards";
	public static final String MSG_WARNING_ACESBLTY_WTRMRKS = "Watermarks are not recommended as per PDF accessibility standards";
	public static final String MSG_WARNING_ACESBLTY_TBULR_STRUCT = "If you want to make table structure then create proper tabular structure format";
	public static String MSG_TEMPLATE_TEXT ="";
	public static final String MSG_ERROR_CUSTMCMNDS = "Remove custom commands for the successful build";
	public static final String MSG_ERROR_INVALID_BUILDER = "Invalid MikTex builder";
	
	public static final String CMND_KILL_TTS = "TASKKILL /F /IM TTS_server_alap_alpha_v1.exe";
	
	public static final String MSG_BUILD_STARTED = "Build Started";
	public static final String MSG_NO_ERROR_FOUND = "No error found";
	public static final String MSG_VBRST_CHNG_CHR = "Verbosity level changed to character";
	public static final String MSG_VBRST_CHNG_WORD = "Verbosity level changed to word";
	public static final String MSG_EMPTY_EDITOR = "There is no text in the editor";
	
	public static final String ALERT_PDF_ACCESSIBLITY_MODE_ON = "Accessible PDF mode  turned on";
	public static final String ALERT_PDF_ACCESSIBLITY_MODE_OFF = "Accessible PDF mode turned off";

	public static final String MSG_SAVE_FILE_FIRST = "Please save the file first";
	public static final String MSG_TTS_ENABLED = "TTS is enabled!";
	public static final String MSG_TTS_DISABLED = "TTS is disabled!";

}
