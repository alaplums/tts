/*
 * $Id$
 *
 * Copyright (c) 2004-2005 by the TeXlapse Team.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 */
package net.sourceforge.texlipse.builder;

import java.awt.font.NumericShaper;
import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileReader;
import java.io.FileWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.io.StringReader;
import java.net.HttpURLConnection;
import java.net.ServerSocket;
import java.net.Socket;
import java.net.URL;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Dictionary;
import java.util.Enumeration;
import java.util.HashMap;
import java.util.Hashtable;
import java.util.List;
import java.util.Map;
import java.util.Stack;
import java.util.regex.Matcher;
import java.util.regex.Pattern;

import net.sourceforge.texlipse.PathUtils;
import net.sourceforge.texlipse.TexlipsePlugin;
import net.sourceforge.texlipse.TTSIntegration.CapsLock;
import net.sourceforge.texlipse.TTSIntegration.TTSConversion;
import net.sourceforge.texlipse.TTSIntegration.TTSProperties;
import net.sourceforge.texlipse.actions.PDFAccessibility;
import net.sourceforge.texlipse.actions.VbstCharAction;
import net.sourceforge.texlipse.editor.TexEditor;
import net.sourceforge.texlipse.model.OutlineNode;
import net.sourceforge.texlipse.model.ParseErrorMessage;
import net.sourceforge.texlipse.model.TexDocumentModel;
import net.sourceforge.texlipse.properties.TexlipseProperties;
import net.sourceforge.texlipse.texparser.LatexLexer;
import net.sourceforge.texlipse.texparser.LatexParser;
import net.sourceforge.texlipse.texparser.TexParser;
import net.sourceforge.texlipse.texparser.node.Token;

import org.eclipse.core.resources.IMarker;
import org.eclipse.core.resources.IResource;
import org.eclipse.core.runtime.CoreException;
import org.eclipse.core.runtime.IStatus;
import org.eclipse.core.runtime.Status;
import org.eclipse.jface.dialogs.ErrorSupportProvider;
import org.eclipse.jface.preference.IPreferenceStore;
import org.eclipse.jface.text.BadLocationException;
import org.eclipse.jface.text.IDocument;
import org.eclipse.swt.widgets.Display;
import org.eclipse.ui.IEditorActionDelegate;
import org.eclipse.ui.IWorkbenchPage;
import org.eclipse.ui.PlatformUI;
import org.eclipse.ui.texteditor.IDocumentProvider;
import org.eclipse.ui.texteditor.ITextEditor;
import org.eclipse.ui.texteditor.MarkerUtilities;

/**
 * Helper methods for external program runners.
 * 
 * @author Kimmo Karlsson
 */
public abstract class AbstractProgramRunner implements ProgramRunner {

	// the currently running program
	private ITextEditor textEditor;
	private static ExternalProgram extrun;  
	private static List<Integer> errorsLine;
	private static List<String> messages;
	private static boolean isErrors= false;
	StringBuffer stringServer = new StringBuffer();
	StringBuffer strEditor = new StringBuffer();
	StringBuffer createPDFToolTipDocument = new StringBuffer();
	private List<ParseErrorMessage> errors;
	private Socket  socket=null;
	
	private static List<ParseErrorMessage> compileErrorStaticList = new  ArrayList<ParseErrorMessage>();
	public static List<ParseErrorMessage> finalErrorList = new  ArrayList<ParseErrorMessage>();;


	public static boolean isErrors() {
		
		return isErrors;
	}

	public static void setErrors(boolean haveErrors) {
		AbstractProgramRunner.isErrors = haveErrors;
	}
	
	public List<ParseErrorMessage> getErrors(){
		return errors;
	}

	/**
	 * Create a new program runner.
	 * 
	 * @param project
	 *            the project holding the properties
	 * @param propertyName
	 *            name of the property containing the name of the program
	 */
	protected AbstractProgramRunner() {
		extrun = new ExternalProgram();
	}

	/**
	 * @return the name of the program runner arguments -preference in the
	 *         plugin preferences
	 */
	private String getArgumentsPreferenceName() {
		return getClass() + "_args";
	}

	/**
	 * @return the name of the program runner path -preference in the plugin
	 *         preferences
	 */
	private String getCommandPreferenceName() {
		return getClass() + "_prog";
	}

	/**
	 * @return the program path and filename from the preferences
	 */
	public String getProgramPath() {
		return TexlipsePlugin.getPreference(getCommandPreferenceName());
	}

	/**
	 * @param path
	 *            the program path and filename for the preferences
	 */
	public void setProgramPath(String path) {
		TexlipsePlugin.getDefault().getPreferenceStore().setValue(getCommandPreferenceName(), path);
	}

	/**
	 * Read the command line arguments for the program from the preferences. The
	 * input filename is marked with a "%input" and the output file name is
	 * marked with a "%output".
	 * 
	 * @return the command line arguments for the program
	 */
	public String getProgramArguments() {
		return TexlipsePlugin.getPreference(getArgumentsPreferenceName());
	}

	/**
	 * @param args
	 *            the program arguments for the preferences
	 */
	public void setProgramArguments(String args) {
		TexlipsePlugin.getDefault().getPreferenceStore().setValue(getArgumentsPreferenceName(), args);
	}

	/**
	 *
	 */
	public void initializeDefaults(IPreferenceStore pref, String path) {
		pref.setDefault(getCommandPreferenceName(), path);
		pref.setDefault(getArgumentsPreferenceName(), getDefaultArguments());
	}

	/**
	 * Returns the default value for the program arguments -preference.
	 * 
	 * @return the program arguments
	 */
	protected String getDefaultArguments() {
		return "%input";
	}

	/**
	 * @return the name of the executable program
	 */
	public String getProgramName() {
		String os = System.getProperty("os.name").toLowerCase();
		if (os.indexOf("windows") >= 0) {
			return getWindowsProgramName();
		} else {
			return getUnixProgramName();
		}
	}

	protected abstract String getWindowsProgramName();

	protected abstract String getUnixProgramName();

	/**
	 * @param resource
	 *            the input file to be processed
	 * @return the arguments to give to the external program
	 */
	protected String getArguments(IResource resource) {
		String args = getProgramArguments();
		if (args == null) {
			return null;
		}
		String ext = resource.getFileExtension();
		String name = resource.getName();
		String baseName = name.substring(0, name.length() - ext.length());
		String inputName = baseName + getInputFormat();
		String outputName = baseName + getOutputFormat();
		if (baseName.indexOf(' ') >= 0) {
			inputName = "\"" + inputName + "\"";
			outputName = "\"" + outputName + "\"";
		}

		if (args.indexOf("%input") >= 0) {
			args = args.replaceAll("%input", inputName);
		}
		if (args.indexOf("%output") >= 0) {
			args = args.replaceAll("%output", outputName);
		}
		if (args.indexOf("%fullinput") >= 0) {
			args = args.replaceAll("%fullinput",
					resource.getParent().getLocation().toFile().getAbsolutePath() + File.separator + inputName);
		}
		if (args.indexOf("%fulloutput") >= 0) {
			args = args.replaceAll("%fulloutput",
					resource.getParent().getLocation().toFile().getAbsolutePath() + File.separator + outputName);
		}
		return args;
	}

	/**
	 * Parse errors from the output of an external program.
	 * 
	 * @param resource
	 *            the input file that was processed
	 * @param output
	 *            the output of the external program
	 * @return true, if error messages were found in the output, false otherwise
	 */
	protected abstract boolean parseErrors(IResource resource, String output);

	/**
	 * Check to see if this program is ready for operation.
	 * 
	 * @return true if this program exists
	 */
	public boolean isValid() {
		if (getProgramPath() == null) {
			return false;
		}
		File f = new File(getProgramPath());
		return f.exists() && f.isFile();
	}

	/**
	 * The main method.
	 * 
	 * @param resource
	 *            the input file to feed to the external program
	 * @throws CoreException
	 *             if the external program is not found or if there was an error
	 *             during the build
	 */
	
	
	boolean IsdocumentRead = false;
	int GetcountTab = 0;
	int GetHSpaceCount=0;
	int lineNumber=0;
	boolean IsFirstLineStatusTab=false;
	boolean IsFirstLineStatusHSpace=false; 
	int firstLineval =0;
    int SecondLineval =0;
    IResource resource = null;
    boolean testing = false;
	public void GetTabSpacing(String DocumentLine,IResource resource)

	{
		
	     lineNumber++;
		   if(IsdocumentRead)
	        {
	        		if(DocumentLine.trim().length() >0)
		        	{
	        			String LineTrim = DocumentLine.trim();
	        			if(!LineTrim.startsWith("%"))
	        			{
	        				
	        			
	        					boolean IsTab = LineTrim.toLowerCase().contains("\\tab");
	        					boolean IsHSpace = LineTrim.toLowerCase().contains("\\hspace");
	        					GetcountTab++;
	        					if(GetcountTab==1)
	        					{
	        						IsFirstLineStatusTab = IsTab;
//	        						IsFirstLineStatusHSpace = IsHSpace;
	        						
	        						if(IsHSpace)
	        						{
	        							String[] words = LineTrim.split(" ");
	        							for(String word : words)
	        							{
	        								if(word.trim().length() >0)
	        								{
	        									if(word.toLowerCase().contains("\\hspace"))
	        									{
	        										String [] str = word.split((""));
	        										for(String a : str)
	        										{  
	        											if(a.matches("-?\\d+(\\.\\d+)?"))
	        											{
	        												 firstLineval = firstLineval + Integer.parseInt(a);
	        												 
	        											}
	        												
	        										}
	        											
	        									}
	        								}
	        							}
	        						}
	        					}
	        					else if(GetcountTab==2)
	        					{
	        						if(IsTab)
	        						{
	        							 AbstractProgramRunner.createMarker(resource, lineNumber,
						       						TTSProperties.MSG_WARNING_ACESBLTY_TBULR_STRUCT, IMarker.SEVERITY_WARNING);
						        		
						        	    try {
											TTSConversion.getDefault().speak(TTSProperties.MSG_WARNING_ACESBLTY_TBULR_STRUCT);
										} catch (IOException e) {
											// TODO Auto-generated catch block
											e.printStackTrace();
										} 
	        						}
	        						
	        						if(IsHSpace)
	        						{
	        							String[] words = LineTrim.split(" ");
	        							for(String word : words)
	        							{
	        								if(word.trim().length() >0)
	        								{
	        									if(word.toLowerCase().contains("\\hspace"))
	        									{
	        										String [] str = word.split((""));
	        										for(String a : str)
	        										{  
	        											if(a.matches("-?\\d+(\\.\\d+)?"))
	        											{
	        												SecondLineval = SecondLineval + Integer.parseInt(a);
	        												 
	        											}
	        												
	        										}
	        											
	        									}
	        								}
	        							}
	        							if(firstLineval == SecondLineval)
	        							{
	        								AbstractProgramRunner.createMarker(resource, lineNumber,
	        										TTSProperties.MSG_WARNING_ACESBLTY_TBULR_STRUCT, IMarker.SEVERITY_WARNING);
						        		
						        	    try {
											TTSConversion.getDefault().speak(TTSProperties.MSG_WARNING_ACESBLTY_TBULR_STRUCT);
										} catch (IOException e) {
											// TODO Auto-generated catch block
											e.printStackTrace();
										} 
	        							}
	        						}
	        						   IsdocumentRead= false;
	        					}
	        				}
		        	}
	        }
	        if(DocumentLine.contains("\\begin{document}") || ("\\begin{document}").equals(DocumentLine))
	        {
	        	IsdocumentRead = true;
	        }
		
	}
	
	public void run(IResource resource) throws CoreException {
		
		
		compileErrorStaticList = new  ArrayList<ParseErrorMessage>();
		File sourceDir = resource.getLocation().toFile().getParentFile();

		// find executable file
		String programPath = getProgramPath();
		File exec = new File(programPath); 
		if (!exec.exists()) { 
			throw new CoreException(TexlipsePlugin.stat("External program (" + programPath + ") not found"));
		}

		// split command into array 
		ArrayList list = new ArrayList();
		list.add(exec.getAbsolutePath());
		PathUtils.tokenizeEscapedString(getArguments(resource), list);
		String[] command = (String[]) list.toArray(new String[0]); 
		
//		String EditorText = textEditor.getDocumentProvider().getDocument(textEditor.getEditorInput()).get();
//		if (textEditor == null)
//			return;
//		TexEditor editor = new TexEditor(); 
//		EditorText = textEditor.getDocumentProvider().getDocument(textEditor.getEditorInput()).get();
//		try { 
//			if (EditorText.isEmpty()) {
//				TTSConversion.getDefault().speak("There is no text in the editor");
//			}  
//			else if (editor.isSaved() && !AbstractProgramRunner.isErrors())
//			{
//				TTSConversion.getDefault().speak(EditorText);
//			} 
//			else 
//			{ 
//				TTSConversion.getDefault().speak("Please save the file first");
//			}
//			//Disable caps lock when shorkey pressed  
//			CapsLock.disableCapsLock();
//		} catch (IOException e) {
//			// TODO Auto-generated catch block
//			e.printStackTrace();
//		}
		
		File fi= new File(resource.getLocation().toFile().toString());
		FileReader fileReaderfont = null;
		try {
			fileReaderfont = new FileReader(fi);
		} catch (FileNotFoundException e2) {
			// TODO Auto-generated catch block
			e2.printStackTrace(); 
		} 
		
		
 		if(PDFAccessibility.isPDFAccessibilityModeOn() && testing)
		{
// 			if(!TexDocumentModel.isParsingError)
// 			if(errorsLine.get(0).intValue() > 0)
 			// if(TTSConversion.getDefault().isTTSPowerFlag() && !(severity == 1) && errorsLine.get(0).intValue() > 0)
 			//
 			//errorsLine = new ArrayList<Integer>();
// 			messages = new ArrayList<String>();
// 			if(errorsLine.get(0).intValue() <= 0)
 		  if(!TexParser.fatalErrors) 
 			{
 				try   
 				{
// 					TexEditor editor = new TexEditor(); 
// 					EditorText = textEditor.getDocumentProvider().getDocument(textEditor.getEditorInput()).get();
 					
 					//if(!isErrors())
 					
 					// read file line by line
 					System.err.println("Read PDF");
 				String documentServer=""; 
 				File file = new File(resource.getLocation().toFile().toString());
 				FileReader fileReader = new FileReader(file);
 				BufferedReader bufferedReader = new BufferedReader(fileReader);
 				String line; 
 				String LatexDocument="";
 				boolean containsCustom = false;
 				while ((line = bufferedReader.readLine()) != null) {
 					innerloop:
 					for (String defCommand : LatexParser.definedCommandsList) { 
 						if(line.contains("\\" + defCommand)){
 							containsCustom = true;
 							break innerloop;
 						}
 					}
 					if(!containsCustom){
 						
 						strEditor.append(line);
 						strEditor.append("\n");
 					}else{
 						strEditor.append("*customcode*" + line);
 						strEditor.append("\n");
 					}
 					containsCustom = false;
 					GetTabSpacing(line,resource);
 				}
// 				while ((line = bufferedReader.readLine()) != null) {
// 				  
// 					 strEditor.append(line);
// 				        strEditor.append("\n");
// 				}
 				documentServer = strEditor.toString();
 				PDFAccessibilityCheck pdfAccessibilityCheck = new PDFAccessibilityCheck(strEditor.toString().toLowerCase(), resource);
 				pdfAccessibilityCheck.performChecks();
 				
 				try {
 					Thread.sleep(4000);
 				} catch (InterruptedException e) {
 					// TODO Auto-generated catch block
 					e.printStackTrace();
 				}
 				
// 				strEditor.setLength(0); 
 				TTSConversion.getDefault().speak("**(LatexDocument)**");
 				
			try {
 					Thread.sleep(4000); 
 				} catch (InterruptedException e) {    
 					// TODO Auto-generated catch block
 					e.printStackTrace();
 				}
 				
// 			System.out.print("send to server abstract Programmer Runner\n"+documentServer.toString());
			 
			   // TTSConversion.getDefault().speak(EditorText.toString());  
				TTSConversion.getDefault().speak(documentServer.toString());  
 				documentServer="";
 				// received data from server in maths equation bind with alt text
 				
 				try {
 					Thread.sleep(500); 
 				} catch (InterruptedException e) {
 					// TODO Auto-generated catch block
 					e.printStackTrace();
 				}
 				
 				TTSConversion.getDefault().ReceivedDataFromServer();
 				fileReader.close(); 
 				// phase-1	 file write bind alt text and write in editor
 				FileWriter f = new FileWriter(resource.getLocation().toFile().toString());
 				f.write(TTSConversion.strLatexDocumentfromServer);
 				f.flush();
 				f.close(); 
 				TTSConversion.strLatexDocumentfromServer="";
 				
 			} catch (IOException e) {
 				e.printStackTrace();
 			} catch (Exception e) {
 				// TODO Auto-generated catch block
 				e.printStackTrace();
 			}
			}
 			else
 	 		{
 	 			// if error case then editor show your own text 
 	 			File file = new File(resource.getLocation().toFile().toString());
 	 			FileReader fileReader = null;
 				try {
 					fileReader = new FileReader(file);
 				} catch (FileNotFoundException e) {
 					// TODO Auto-generated catch block
 					e.printStackTrace();
 				}
 	 			BufferedReader bufferedReader = new BufferedReader(fileReader);
 	 			String line; 
 	 			try {
 					while ((line = bufferedReader.readLine()) != null) {
 					  		strEditor.append(line);
 							strEditor.append("\n");
 					}
 				} catch (IOException e) {
 					// TODO Auto-generated catch block
 					e.printStackTrace();
 				}
 	 		}
		}
 		
		
		// check if we are using console 
 String console = null;  
		if (TexlipsePlugin.getDefault().getPreferenceStore().getBoolean(TexlipseProperties.BUILDER_CONSOLE_OUTPUT)) {
			console = getProgramName();
		}
		extrun.setup(command, sourceDir, console);

		String output = null;
	    String outputSecond = null;
		try { 

			String[] query = getQueryString();
			if (query != null) {
				output = extrun.run(query);
			} else {
				output = extrun.run();
			}

		} catch (Exception e) {
			throw new CoreException(new Status(IStatus.ERROR, TexlipsePlugin.getPluginId(), IStatus.ERROR,
					"Building the project: ", e));
		} finally {
			extrun.stop();
		}
		errorsLine = new ArrayList<Integer>();
		messages = new ArrayList<String>();
		if(PDFAccessibility.isPDFAccessibilityModeOn())
		{
			
			//checking errors on ctrl+S for PDF Mode
			if(!TexParser.fatalErrors) 
			{
			if(!output.contains("<inserted text>") && !output.contains("! LaTeX Error: Missing"))
			{
				String documentServer=""; 
				StringBuffer strTemp = new StringBuffer();
					File file = new File(resource.getLocation().toFile().toString());
					FileReader fileReader = null;
					try {
						fileReader = new FileReader(file);
					} catch (FileNotFoundException e3) { 
						// TODO Auto-generated catch block
						e3.printStackTrace(); 
					}
					BufferedReader bufferedReader = new BufferedReader(fileReader);
					String line=""; 
					String LatexDocument="";
					boolean containsCustom = false;
					try {
						while ((line = bufferedReader.readLine()) != null) {
							innerloop:
			 					for (String defCommand : LatexParser.definedCommandsList) { 
			 						if(line.contains("\\" + defCommand)){
			 							containsCustom = true;
			 							break innerloop;
			 						}
			 					}
			 					if(!containsCustom){
			 						
			 						strEditor.append(line);
			 						strEditor.append("\n");
			 					}else{
			 						strEditor.append("*customcode*" + line);
			 						strEditor.append("\n");
			 					}
			 					containsCustom = false;
			 					GetTabSpacing(line,resource);
						}
					} catch (IOException e2) {
						// TODO Auto-generated catch block
						e2.printStackTrace();
					}
					
					documentServer = strEditor.toString();
					PDFAccessibilityCheck pdfAccessibilityCheck = new PDFAccessibilityCheck(strEditor.toString().toLowerCase(), resource);
	 				pdfAccessibilityCheck.performChecks();
	 				try {
	 					TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_PDFACESBLTY_MODE);
						
					} catch (IOException e3) {
						// TODO Auto-generated catch block
						e3.printStackTrace();
					}
	 				try {
	 					Thread.sleep(500);
	 				} catch (Exception e1) {
	 					// TODO Auto-generated catch block
	 					e1.printStackTrace();
	 				}
	 				
					try {
						TTSConversion.getDefault().speak(documentServer.toString());
					} catch (IOException e3) {
						// TODO Auto-generated catch block
						e3.printStackTrace();
					}
	 				documentServer="";
	 				try {
						TTSConversion.getDefault().ReceivedDataFromServer();
					} catch (IOException e2) {
						// TODO Auto-generated catch block
						e2.printStackTrace();
					}
	 				try {
						fileReader.close();
					} catch (IOException e2) {
						// TODO Auto-generated catch block
						e2.printStackTrace();
					} 
	 			// phase-1	 file write bind alt text and write in editor
				FileWriter f = null;
				try {
					f = new FileWriter(resource.getLocation().toFile().toString());
				} catch (IOException e1) {
					// TODO Auto-generated catch block
					e1.printStackTrace();
				}
					try {
						f.write(TTSConversion.strLatexDocumentfromServer);
					} catch (IOException e1) {
						// TODO Auto-generated catch block
						e1.printStackTrace();
					}
					try {
						f.flush();
					} catch (IOException e1) {
						// TODO Auto-generated catch block
						e1.printStackTrace();
					}
					try {
						f.close();
					} catch (IOException e1) {
						// TODO Auto-generated catch block
						e1.printStackTrace();
					} 
					TTSConversion.strLatexDocumentfromServer="";
					sourceDir = resource.getLocation().toFile().getParentFile();
					extrun = new ExternalProgram();
//					extrun.run();
				extrun.setup(command, sourceDir, console);
				String out=null;
				try { 

					String[] query = getQueryString();
					if (query != null) {
						outputSecond = extrun.run(query);
					//	System.err.println(outputSecond); 
					} else {
						outputSecond = extrun.run();
					}

				} catch (Exception e) {
					throw new CoreException(new Status(IStatus.ERROR, TexlipsePlugin.getPluginId(), IStatus.ERROR,
							"Building the project: ", e)); 
				} finally {
					extrun.stop();
				}

			}
			else
			{
				// if run time error case then editor show your own text 
	 			File file = new File(resource.getLocation().toFile().toString());
	 			FileReader fileReader = null;
				try {
					fileReader = new FileReader(file);
				} catch (FileNotFoundException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
	 			BufferedReader bufferedReader = new BufferedReader(fileReader);
	 			String line; 
	 			try {
					while ((line = bufferedReader.readLine()) != null) {
					  		strEditor.append(line);
							strEditor.append("\n");
					}
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}
			}
		else
		{
			// if error case then editor show your own text 
	 			File file = new File(resource.getLocation().toFile().toString());
	 			FileReader fileReader = null;
				try {
					fileReader = new FileReader(file);
				} catch (FileNotFoundException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
	 			BufferedReader bufferedReader = new BufferedReader(fileReader);
	 			String line; 
	 			try {
					while ((line = bufferedReader.readLine()) != null) {
					  		strEditor.append(line);
							strEditor.append("\n");
					}
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
	    	}
		}
		if(PDFAccessibility.isPDFAccessibilityModeOn())
		{
			// file write show user 
		FileWriter fa = null;
		try {
			fa = new FileWriter(resource.getLocation().toFile().toString());
		} catch (IOException e1) {
			// TODO Auto-generated catch block
			e1.printStackTrace();
		}
		try {
			//modify strEditor to remove *customcode*
			String modStrEditor = strEditor.toString().replace("*customcode*", " ");
			fa.write(modStrEditor.toString());
		} catch (IOException e1) { 
			// TODO Auto-generated catch block
			e1.printStackTrace();
		}
		try {
			fa.close();
		} catch (IOException e1) {
			// TODO Auto-generated catch block
			e1.printStackTrace();
		}
		strEditor.setLength(0);
		}
		
 		if (!parseErrors(resource, output)) {			 
			try {
				setErrors(false); 
				TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_SPECIAL_CMND);
				Thread.sleep(500);
				TTSConversion.getDefault().speak(TTSProperties.MSG_NO_ERROR_FOUND);
				//System.out.println("Abstract Programmer Runner : No Error");  
			} catch (IOException e) { 
				// TODO Auto-generated catch block 
				e.printStackTrace(); 
			} catch (InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}			
		}
		else 
		{
			combineErrorsList();
			setErrors(true);
			throw new BuilderCoreException(TexlipsePlugin.stat("Errors during build. See the problems dialog."));
		}
	}

//	private void GetTabSpacing() {
//		// TODO Auto-generated method stub
//		
//	}

	/**
	 * Kill the external program if it is running.
	 */
	public void stop() {
		if (extrun != null) {
			extrun.stop();
		}
	}

	/**
	 * Returns a special query string that indicates that this program is
	 * waiting an input from the user.
	 * 
	 * @return the query string to look for in the output of the program
	 */
	protected String[] getQueryString() {
		return null;
	}

	/**
	 * Create a layout warning marker to the given resource.
	 *
	 * @param resource
	 *            the file where the problem occurred
	 * @param message
	 *            error message
	 * @param lineNumber
	 *            line number
	 * @param markerType
	 * @param severity
	 *            Severity of the error
	 */
	@SuppressWarnings("checked")
	protected static void createMarker(IResource resource, final Integer lineNumber, String message, String markerType,
			int severity) {
		int lineNr = -1;
		if (lineNumber != null) {
			lineNr = lineNumber;
		} 

		// System.out.println(lineNr);
		IMarker marker = AbstractProgramRunner.findMarker(resource, lineNr, message, markerType);
		if (marker == null) {
			try {
				compileErrorStaticList.add(new ParseErrorMessage(lineNr, 0, 0,
						message, severity));
				HashMap map = new HashMap();				
				map.put(IMarker.MESSAGE, message);
				map.put(IMarker.SEVERITY, new Integer(severity));				
				if (lineNumber != null)
					map.put(IMarker.LINE_NUMBER, lineNumber);
				errorsLine = new ArrayList<Integer>();
				messages = new ArrayList<String>();
				MarkerUtilities.createMarker(resource, map, markerType);
				errorsLine.add(lineNr); 
				messages.add(message); 
			} catch (CoreException e) {
				throw new RuntimeException(e);
			} 
		}				

		// goto error
		Display.getDefault().asyncExec(new Runnable() {
			public void run() {
				ITextEditor editor = (ITextEditor) PlatformUI.getWorkbench().getActiveWorkbenchWindow().getActivePage()
						.getActiveEditor();

				IDocumentProvider provider = editor.getDocumentProvider();
				IDocument document = provider.getDocument(editor.getEditorInput());
				try {
   
					int start = document.getLineOffset(errorsLine.get(0) - 1);
					editor.selectAndReveal(start, 0);

					IWorkbenchPage page = editor.getSite().getPage();
					page.activate(editor);

				} catch (BadLocationException x) {
					// ignore
				}
			}
		});
		

		//block the warnnings and unnecessary errors to show in the problem window
	 if (TTSConversion.getDefault().isTTSPowerFlag() && !(severity == 1) && errorsLine.get(0).intValue() > 0) {
		try {
			TTSConversion.getDefault().stop(TTSProperties.SEND_DATA_NEXT);
			Thread.sleep(1000);
			TTSConversion.getDefault().speak(messages.get(0) + " at line number " + errorsLine.get(0));
			//System.err.println("Abstract Programmer Runner: Error Find");
			Thread.sleep(1000);
		} catch (IOException e) {  
			// TODO Auto-generated catch block
			e.printStackTrace();
		} catch (Exception e) {

		}
	}
	 
	}

	/**
	 * Create a layout warning marker to the given resource.
	 *
	 * @param resource 
	 *            the file where the problem occured
	 * @param message
	 *            error message
	 * @param lineNumber
	 *            line number
	 */
	public static void createLayoutMarker(IResource resource, Integer lineNumber, String message) {
		String markerType = TexlipseBuilder.LAYOUT_WARNING_TYPE;
		int severity = IMarker.SEVERITY_WARNING;
		createMarker(resource, lineNumber, message, markerType, severity);
	}

	/**
	 * Create a marker to the given resource.
	 * 
	 * @param resource
	 *            the file where the problem occured
	 * @param message
	 *            error message
	 * @param lineNumber
	 *            line number
	 * @param severity
	 *            severity of the marker
	 */
	public static void createMarker(IResource resource, Integer lineNumber, String message, int severity) {
		String markerType = TexlipseBuilder.MARKER_TYPE;
		createMarker(resource, lineNumber, message, markerType, severity);
	}

	/**
	 * Create a marker to the given resource. The marker's severity will be
	 * "ERROR".
	 * 
	 * @param resource
	 *            the file where the problem occured
	 * @param message
	 *            error message
	 * @param lineNumber
	 *            line number
	 */
	public static void createMarker(IResource resource, Integer lineNumber, String message) {
		createMarker(resource, lineNumber, message, IMarker.SEVERITY_ERROR);
	}

	/**
	 * Checks pre-existance of marker.
	 * 
	 * @param resource
	 *            Resource in which marker will searched
	 * @param lineNr
	 *            IMarker.LINE_NUMBER of the marker
	 * @param message
	 *            Message for marker
	 * @param type
	 *            The type of the marker to find
	 * @return pre-existance of marker or null if no marker was found
	 */
	public static IMarker findMarker(IResource resource, int lineNr, String message, String type) {

		try {
			IMarker[] tasks = resource.findMarkers(type, true, IResource.DEPTH_ZERO);
			for (IMarker marker : tasks) {
				Object lNrObj = marker.getAttribute(IMarker.LINE_NUMBER);
				int lNr = -1;
				if (lNrObj != null) {
					lNr = ((Integer) lNrObj);
				}
				if (lNr == lineNr && marker.getAttribute(IMarker.MESSAGE).equals(message)) {
					return marker;
				}
			}
		} catch (CoreException e) {
			throw new RuntimeException(e);
		}
		return null;
	}
	
	public void combineErrorsList(){
		finalErrorList = new ArrayList<ParseErrorMessage>();
		Stack<ParseErrorMessage> errorStack = new Stack<ParseErrorMessage>();
		List<ParseErrorMessage> parseErrors = LatexParser.getErrorsStatic();
		for(ParseErrorMessage error : parseErrors){ //add compile time errors
			if(error.getSeverity() == 2){
				errorStack.push(error);
			}
		}
		for(ParseErrorMessage error : compileErrorStaticList){ //add run time errors
			if(error.getSeverity() == 2){
				errorStack.push(error);
			}
		}
		
		finalErrorList = new ArrayList<ParseErrorMessage>(errorStack);
		Collections.reverse(finalErrorList);
		
	}
}

