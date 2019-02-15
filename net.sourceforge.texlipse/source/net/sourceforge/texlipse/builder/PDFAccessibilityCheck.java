package net.sourceforge.texlipse.builder;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.StringReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Stack;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import org.eclipse.core.resources.IMarker;
import org.eclipse.core.resources.IResource;

import net.sourceforge.texlipse.TTSIntegration.TTSConversion;
import net.sourceforge.texlipse.TTSIntegration.TTSProperties;
import net.sourceforge.texlipse.model.ParseErrorMessage;
import net.sourceforge.texlipse.texparser.LatexParser;

public class PDFAccessibilityCheck {

	String documentText;
	IResource resource;

	PDFAccessibilityCheck(String documentText, IResource resource) {
		this.documentText = documentText;
		this.resource = resource;
	}

	// forms
	private List<String> formFieldsList = new ArrayList<String>();
	int formFieldsCount;
	int toolTipCount;
	int wrappedInToolTip;
	static boolean isToolTipMissing = false;

	// url
	Map<String, Integer> urlLineNumberMap = new HashMap<String, Integer>();
	Stack<Character> urlStack = new Stack<Character>();
	Stack<Character> preservedUrlStack = new Stack<Character>();

	private List<ParseErrorMessage> errors;

	public void performChecks() {
		if (documentText.contains("hyperref") || documentText.contains("comment")
				|| documentText.contains("*customcode*")) // package for forms
															// and url
			handleForms();
	}

	private void handleForms() {
		int lineNumber = 0;
		populateFormFieldsList();
		// tooltip check
		BufferedReader reader = new BufferedReader(new StringReader(documentText));

		try {
			String line;
			while ((line = reader.readLine()) != null) {
				lineNumber++;
				if (line != "" && !line.startsWith("%")) {
					if (line.contains("\\begin{form}")) {
						while (!line.contains("\\end{form}") && (line = reader.readLine()) != null) {
							// line = reader.readLine();
							if (line.startsWith("%"))
								continue;
							lineNumber++;
							for (String command : formFieldsList) {
								if (line.contains(command)) {
									formFieldsCount++;
									if (line.contains("\\pdftooltip")) {
										wrappedInToolTip++;
									}

									// label not missing
									if (line.contains("{") && line.contains("}")) {
										String labelSubstring = line.substring(line.indexOf("{") + 1,
												line.indexOf("}"));
										if (labelSubstring.length() <= 0) {
											AbstractProgramRunner.createMarker(resource, lineNumber,
													TTSProperties.MSG_WARNING_ACESBLTY_FORM_LABELS,
													IMarker.SEVERITY_WARNING);
											alertTTS();
											TTSConversion.getDefault().speak(
													TTSProperties.MSG_WARNING_ACESBLTY_FORM_LABELS);
										}
									}

								}
							}
							if (line.contains("\\pdftooltip")) {
								toolTipCount++;
							}

							// hidden attribute used
							String noSpacesLine = line.replaceAll("\\s", "");
							if (noSpacesLine.contains("hidden=true")) {
								AbstractProgramRunner.createMarker(resource, lineNumber,
										TTSProperties.MSG_WARNING_ACESBLTY_FORM_HIDDEN_FIELDS,
										IMarker.SEVERITY_WARNING);
								alertTTS();
								TTSConversion.getDefault()
										.speak(TTSProperties.MSG_WARNING_ACESBLTY_FORM_HIDDEN_FIELDS);
							}

						}
						if (toolTipCount < formFieldsCount || wrappedInToolTip < formFieldsCount) {
							isToolTipMissing = true;
							AbstractProgramRunner.createMarker(resource, lineNumber,
									TTSProperties.MSG_WARNING_ACESBLTY_FORM_TOOLTIP,
									IMarker.SEVERITY_WARNING);
							alertTTS();
							TTSConversion.getDefault()
									.speak(TTSProperties.MSG_WARNING_ACESBLTY_FORM_TOOLTIP);
							isToolTipMissing = false;
							toolTipCount = 0;
							formFieldsCount = 0;
							wrappedInToolTip = 0;
						}

					}
					if (line.contains("comment"))
						handleComments(line, lineNumber);
					handleURL(line, lineNumber);
					handleCustomCommands(line, lineNumber);
				}
			}

		} catch (IOException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	private void alertTTS() throws IOException {
		TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_NEXT);
		try {
			Thread.sleep(500);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	private void populateFormFieldsList() {
		if (formFieldsList.size() <= 0) {
			formFieldsList.add("\\textfield");
			formFieldsList.add("\\checkbox");
			formFieldsList.add("\\choicemenu");
			formFieldsList.add("\\pushbutton");
			formFieldsList.add("\\submit");
			formFieldsList.add("\\reset");
		}
	}

	private void handleComments(String line, int lineNumber) {
		if (!line.contains("\\usepackage")) { // to avoid warning at the package
												// use of pdfcomment
			String pattern = "\\\\pdf\\w+comment";
			Pattern r = Pattern.compile(pattern);
			Matcher m = r.matcher(line);

			try {
				if (m.find()) {
					AbstractProgramRunner.createMarker(resource, lineNumber,
							TTSProperties.MSG_WARNING_ACESBLTY_COMENTS, IMarker.SEVERITY_WARNING);
					alertTTS();
					TTSConversion.getDefault()
							.speak(TTSProperties.MSG_WARNING_ACESBLTY_COMENTS);
					return;
				}

				if (line.contains("\\pdfcomment")) {
					AbstractProgramRunner.createMarker(resource, lineNumber,
							TTSProperties.MSG_WARNING_ACESBLTY_COMENTS, IMarker.SEVERITY_WARNING);
					alertTTS();
					TTSConversion.getDefault()
							.speak(TTSProperties.MSG_WARNING_ACESBLTY_COMENTS);
				}
			} catch (Exception ex) {

			}

		}
	}

	private void handleURL(String line, int lineNumber) {
		String noSpacesLine = line.replaceAll("\\s", "");
		if ((noSpacesLine.contains("\\url{") || noSpacesLine.contains("\\href{"))) {
			String command = line.contains("\\url") ? "\\url" : "\\href";
			String subLine = line.substring(line.indexOf(command) + command.length()).trim();
			extractURL(subLine, lineNumber);
		} else if (urlStack.size() > 0) {
			extractURL(line, lineNumber);
		}
		try {
			if (urlLineNumberMap.size() > 0) {
				for (Map.Entry<String, Integer> entry : urlLineNumberMap.entrySet()) {
					if (!entry.getKey().startsWith("run:") && !entry.getKey().startsWith("mailto:")) {// skip
																										// local
																										// addresses
						if (pingURL(entry.getKey()) != 1) {
							AbstractProgramRunner.createMarker(resource, lineNumber,
									TTSProperties.MSG_WARNING_ACESBLTY_URL,
									IMarker.SEVERITY_WARNING);
							urlLineNumberMap.remove(entry.getKey(), entry.getValue());
							alertTTS();
							TTSConversion.getDefault()
									.speak(TTSProperties.MSG_WARNING_ACESBLTY_URL);
						}
					}
				}
			}
		} catch (Exception ex) {
		}
	}

	private void extractURL(String subLine, int lineNumber) {
		String url = "";
		char[] subLineChars = subLine.toCharArray();
		int i = 0;
		while (i < subLineChars.length) {
			char c = subLineChars[i];
			if (c != '}') {
				urlStack.push(c);
				preservedUrlStack.push(c);
				i++;
			} else {
				while (urlStack.peek() != '{') {
					urlStack.pop();
				}
				urlStack.pop();

				if (urlStack.size() > 0) {
					i++;
				} else {
					while (preservedUrlStack.size() != 0) {
						url += preservedUrlStack.pop().toString();
					}
					url = new StringBuilder(url).reverse().toString().substring(1);
					System.out.print(url);
					urlLineNumberMap.put(url, lineNumber);
					break;
				}
			}
		}
	}

	private int pingURL(String url) {
		int result = 0;
		try {
			URL siteURL = new URL(url);
			HttpURLConnection connection = (HttpURLConnection) siteURL.openConnection();
			connection.setRequestMethod("GET");
			connection.connect();

			int code = connection.getResponseCode();
			if (code == 200) {
				result = 1;
			}
		} catch (Exception ex) {
			result = 0;
		}

		return result;
	}

	public void handleCustomCommands(String line, int lineNumber) {
		try{
			if (!line.contains("\\newcommand") && !line.contains("\\renewcommand")) {
				if (line.contains("*customcode*") && !line.contains("\\pdftooltip")) {
					AbstractProgramRunner.createMarker(resource, lineNumber,
							TTSProperties.MSG_WARNING_ACESBLTY_CUSTMCMNDS_TOOLTIP,
							IMarker.SEVERITY_WARNING);
					alertTTS();
					TTSConversion.getDefault()
					.speak(TTSProperties.MSG_WARNING_ACESBLTY_CUSTMCMNDS_TOOLTIP);
				}
			}
		}catch(Exception ex){
			
		}
	}
}
