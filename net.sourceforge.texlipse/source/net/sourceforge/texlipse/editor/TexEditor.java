/*
 * $Id$
 *
 * Copyright (c) 2004-2005 by the TeXlapse Team.
 * All rights reserved. This program and the accompanying materials
 * are made available under the terms of the Eclipse Public License v1.0
 * which accompanies this distribution, and is available at
 * http://www.eclipse.org/legal/epl-v10.html
 */
package net.sourceforge.texlipse.editor;


import org.eclipse.swt.events.KeyEvent;

import java.io.IOException;
import java.util.ArrayList;

import net.sourceforge.texlipse.TexlipsePlugin;
import net.sourceforge.texlipse.TTSIntegration.CapsLock;
import net.sourceforge.texlipse.TTSIntegration.TTSConversion;
import net.sourceforge.texlipse.TTSIntegration.TTSProperties;
import net.sourceforge.texlipse.actions.VbstCharAction;
import net.sourceforge.texlipse.builder.AbstractProgramRunner;
import net.sourceforge.texlipse.model.TexDocumentModel;
import net.sourceforge.texlipse.outline.TexOutlinePage;
import net.sourceforge.texlipse.properties.TexlipseProperties;
import net.sourceforge.texlipse.treeview.views.TexOutlineTreeView;

import org.eclipse.core.resources.IProject;
import org.eclipse.core.resources.IResource;
import org.eclipse.core.runtime.IProgressMonitor;
import org.eclipse.jface.action.IAction;
import org.eclipse.jface.preference.IPreferenceStore;
import org.eclipse.jface.text.BadLocationException;
import org.eclipse.jface.text.IDocument;
import org.eclipse.jface.text.ITextSelection;
import org.eclipse.jface.text.ITextViewerExtension;
import org.eclipse.jface.text.TextSelection;
import org.eclipse.jface.text.source.ISourceViewer;
import org.eclipse.jface.text.source.IVerticalRuler;
import org.eclipse.jface.text.source.Annotation;
import org.eclipse.jface.text.source.projection.ProjectionSupport;
import org.eclipse.jface.text.source.projection.ProjectionViewer;
import org.eclipse.jface.viewers.IPostSelectionProvider;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.jface.viewers.ISelectionChangedListener;
import org.eclipse.jface.viewers.SelectionChangedEvent;
import org.eclipse.swt.SWT;
import org.eclipse.swt.events.KeyListener;
import org.eclipse.swt.widgets.Composite;
import org.eclipse.swt.widgets.Event;
import org.eclipse.ui.IActionDelegate2;
import org.eclipse.ui.IFileEditorInput;
import org.eclipse.ui.editors.text.TextEditor;
import org.eclipse.ui.texteditor.ITextEditorActionDefinitionIds;
import org.eclipse.ui.texteditor.MarkerAnnotation;
import org.eclipse.ui.texteditor.MarkerUtilities;
import org.eclipse.ui.texteditor.SourceViewerDecorationSupport;
import org.eclipse.ui.texteditor.TextOperationAction;
import org.eclipse.ui.views.contentoutline.IContentOutlinePage;

//import com.sun.glass.events.KeyEvent;

/**
 * The Latex editor.
 * 
 * @author Oskar Ojala
 * @author Taavi Hupponen
 * @author Boris von Loesch
 * @author Ahtsham Manzoor
 */
public class TexEditor extends TextEditor implements IActionDelegate2 {

	public final static String TEX_PARTITIONING = "__tex_partitioning";
	private static int lineNumber;
	private static int lastLineNumber;
	private static String currentLine;
    private static int tempLineNumber = 0;
    private static int tempCursorOffset = 0;
	private static char nextCharacter;
	private static boolean isWrtingMode =false;
	private static String forwardCursorPart;
	private static String forwardCursorPartTillEnd;
	private static boolean isCurserKeyPressed = false;
	private static String startFromImmediateWord;
	private static String startFromImmediateWordTillEnd;
	private static String fromStartOfLineToCursor;
	private static String typedWord="";
	private static boolean capsLockStatus = false;
	private static String capsString;
	private static IDocument sourceDocument=null;
	private static int cursorOffset=0;
	private static int lastCursorOffset=0;
	/** The editor's bracket matcher */
	private TexPairMatcher fBracketMatcher = new TexPairMatcher("()[]{}");

	private TexOutlinePage outlinePage;
	private TexOutlineTreeView fullOutline;
	private TexDocumentModel documentModel;
	private TexCodeFolder folder;
	private ProjectionSupport fProjectionSupport;
	private BracketInserter fBracketInserter;
	private TexlipseAnnotationUpdater fAnnotationUpdater;
	private static boolean isSaved= false;
	int errorListIndex = 0;

	public boolean isSaved() {
		return isSaved;
	}

	public void setSaved(boolean isSaved) {
		this.isSaved = isSaved;
	}

	/**
	 * Constructs a new editor. 
	 */
	public TexEditor() {
		super();
		// setRangeIndicator(new DefaultRangeIndicator());
		folder = new TexCodeFolder(this);
	}

	/**
	 * Create the part control.
	 * 
	 * @see org.eclipse.ui.IWorkbenchPart#createPartControl(org.eclipse.swt.widgets.Composite)
	 */
	public void createPartControl(Composite parent) {
		super.createPartControl(parent);

		// enable projection support (for code folder)
		ProjectionViewer projectionViewer = (ProjectionViewer) getSourceViewer();
		fProjectionSupport = new ProjectionSupport(projectionViewer, getAnnotationAccess(), getSharedColors());
		fProjectionSupport.addSummarizableAnnotationType("org.eclipse.ui.workbench.texteditor.error");
		fProjectionSupport.addSummarizableAnnotationType("org.eclipse.ui.workbench.texteditor.warning");
		fProjectionSupport.install();

		if (TexlipsePlugin.getDefault().getPreferenceStore().getBoolean(TexlipseProperties.CODE_FOLDING)) {
			projectionViewer.doOperation(ProjectionViewer.TOGGLE);
		}

		fAnnotationUpdater = new TexlipseAnnotationUpdater(this);

		((IPostSelectionProvider) getSelectionProvider())
				.addPostSelectionChangedListener(new ISelectionChangedListener() {
					public void selectionChanged(SelectionChangedEvent event) {
						// Delete all StatuslineErrors after selection changes
						if(!AbstractProgramRunner.isErrors() && !TexDocumentModel.isParsingError){
							ISelection sel = event.getSelectionProvider().getSelection();
							if (sel instanceof TextSelection){
								ITextSelection textSel = (ITextSelection) sel;
								String selectedText = textSel.getText();
								if(selectedText.length() > 0)
									speakCurserModeText(selectedText);
							}											
							//documentModel.removeStatusLineErrorMessage();	
						}
										
					}
				});
		
		KeyListener wordNavigationKeyListener = new  KeyListener(){
			@Override
			public void keyPressed(KeyEvent e){
				
			}
		
			@Override
			public void keyReleased(KeyEvent e) {
				if (isCurserKeyPressed && !VbstCharAction.getDefault().isVerbosityChar()) { //cursor key is pressed and verbosity is not char
					// TODO Auto-generated method stub
					if (e.keyCode == SWT.ARROW_RIGHT && (e.stateMask & SWT.CTRL) == SWT.CTRL) {

						String curosrText = "";
						String currentLine = getCurrentLine().replace("\t", "    ");
						curosrText = currentLine.substring(lastCursorOffset - 1, cursorOffset - 1);
						System.out.println(curosrText);
						speakCurserModeText(curosrText);
					} else if (e.keyCode == SWT.ARROW_LEFT && (e.stateMask & SWT.CTRL) == SWT.CTRL) {

						String curosrText = "";
						String currentLine = getCurrentLine().replace("\t", "    ");
						curosrText = currentLine.substring(cursorOffset - 1, lastCursorOffset - 1);
						System.out.println(curosrText);
						speakCurserModeText(curosrText);
					}
				}
			}
		};
		
		KeyListener errorListKeyListener = new  KeyListener(){
			@Override
			public void keyPressed(KeyEvent e){
				
			}
		
			@Override
			public void keyReleased(KeyEvent e) {
					if (e.keyCode == java.awt.event.KeyEvent.VK_COMMA && (e.stateMask & SWT.CTRL) == SWT.CTRL) { 
						if(errorListIndex == 0)
							errorListIndex = AbstractProgramRunner.finalErrorList.size() - 1;
						else
							errorListIndex -= 1;
						try {
							TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_NEXT);
							Thread.sleep(500);
							TTSConversion.getDefault().speak(AbstractProgramRunner.finalErrorList.get(errorListIndex).getMsg());
						} catch (InterruptedException | IOException e1) {
							// TODO Auto-generated catch block
							e1.printStackTrace();
						}
						
					} else if (e.keyCode == java.awt.event.KeyEvent.VK_PERIOD && (e.stateMask & SWT.CTRL) == SWT.CTRL) {
						if(errorListIndex < AbstractProgramRunner.finalErrorList.size() - 1)
							errorListIndex += 1;
						else
							errorListIndex = 0 ;
						try {
							TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_NEXT);
							Thread.sleep(500);
							TTSConversion.getDefault().speak(AbstractProgramRunner.finalErrorList.get(errorListIndex).getMsg());
						} catch (InterruptedException | IOException e1) {
							// TODO Auto-generated catch block
							e1.printStackTrace();
						}
						
					}
				}
		};
		getViewer().getTextWidget().addKeyListener(wordNavigationKeyListener);
		getViewer().getTextWidget().addKeyListener(errorListKeyListener);
		// register documentModel as documentListener
		// in initializeEditor this would cause NPE
		this.getDocumentProvider().getDocument(getEditorInput()).addDocumentListener(this.documentModel);
		this.documentModel.initializeModel();
		this.documentModel.updateNow();

		ISourceViewer sourceViewer = getSourceViewer();
		if (sourceViewer instanceof ITextViewerExtension) {
			if (fBracketInserter == null)
				fBracketInserter = new BracketInserter(getSourceViewer(), this);
			((ITextViewerExtension) sourceViewer).prependVerifyKeyListener(fBracketInserter);
		}
	}

	/**
	 * Initialize TexDocumentModel and enable latex support in projects other
	 * than Latex Project.
	 * 
	 * @see org.eclipse.ui.texteditor.AbstractDecoratedTextEditor#initializeEditor()
	 */
	protected void initializeEditor() {
		super.initializeEditor();
		setEditorContextMenuId("net.sourceforge.texlipse.texEditorScope");
		this.documentModel = new TexDocumentModel(this);
		setSourceViewerConfiguration(new TexSourceViewerConfiguration(this));
		// register a document provider to get latex support even in non-latex
		// projects
		if (getDocumentProvider() == null) {
			setDocumentProvider(new TexDocumentProvider());
		}
	}

	/**
	 * Create, configure and return the SourceViewer.
	 * 
	 * @see org.eclipse.ui.texteditor.AbstractTextEditor#createSourceViewer(org.eclipse.swt.widgets.Composite,
	 *      org.eclipse.jface.text.source.IVerticalRuler, int)
	 */
	protected ISourceViewer createSourceViewer(Composite parent, IVerticalRuler ruler, int styles) {
		ProjectionViewer viewer = new ProjectionViewer(parent, ruler, getOverviewRuler(), true, styles);
		getSourceViewerDecorationSupport(viewer);
		return viewer;
	}

	/**
	 * @see org.eclipse.ui.texteditor.AbstractDecoratedTextEditor#configureSourceViewerDecorationSupport(org.eclipse.ui.texteditor.SourceViewerDecorationSupport)
	 */
	protected void configureSourceViewerDecorationSupport(SourceViewerDecorationSupport support) {
		// copy the necessary values from plugin preferences instead of
		// overwriting editor preferences
		getPreferenceStore().setValue(TexlipseProperties.MATCHING_BRACKETS,
				TexlipsePlugin.getPreference(TexlipseProperties.MATCHING_BRACKETS));
		getPreferenceStore().setValue(TexlipseProperties.MATCHING_BRACKETS_COLOR,
				TexlipsePlugin.getPreference(TexlipseProperties.MATCHING_BRACKETS_COLOR));

		support.setCharacterPairMatcher(fBracketMatcher);
		support.setMatchingCharacterPainterPreferenceKeys(TexlipseProperties.MATCHING_BRACKETS,
				TexlipseProperties.MATCHING_BRACKETS_COLOR);

		super.configureSourceViewerDecorationSupport(support);
	}

	/**
	 * @see org.eclipse.ui.texteditor.AbstractTextEditor#createActions()
	 */
	protected void createActions() {
		super.createActions();

		IAction a = new TextOperationAction(TexlipsePlugin.getDefault().getResourceBundle(), "ContentAssistProposal.",
				this, ISourceViewer.CONTENTASSIST_PROPOSALS);
		a.setActionDefinitionId(ITextEditorActionDefinitionIds.CONTENT_ASSIST_PROPOSALS);
		setAction("ContentAssistProposal", a);
	}

	/**
	 * @return The source viewer of this editor
	 */
	public ISourceViewer getViewer() {
		return getSourceViewer();
	}

	/**
	 * Used by platform to get the OutlinePage and ProjectionSupport adapter.
	 * 
	 * @see org.eclipse.core.runtime.IAdaptable#getAdapter(java.lang.Class)
	 */
	public Object getAdapter(Class required) {
		if (IContentOutlinePage.class.equals(required)) {
			if (this.outlinePage == null) {
				this.outlinePage = new TexOutlinePage(this);
				this.documentModel.updateOutline();
			}
			return outlinePage;
		} else if (fProjectionSupport != null) {
			Object adapter = fProjectionSupport.getAdapter(getSourceViewer(), required);
			if (adapter != null)
				return adapter;
		}
		return super.getAdapter(required);
	}

	/**
	 * @return The outline page associated with this editor
	 */
	public TexOutlinePage getOutlinePage() {
		return this.outlinePage;
	}

	/**
	 * @return Returns the documentModel.
	 */
	public TexDocumentModel getDocumentModel() {
		return documentModel;
	}

	/**
	 * @return the preference store of this editor
	 */
	public IPreferenceStore getPreferences() {
		return getPreferenceStore();
	}

	/**
	 * Triggers parsing. If there is a way to determine whether the platform is
	 * currently being shut down, triggering of parsing in such a case could be
	 * skipped.
	 * 
	 * @see org.eclipse.ui.ISaveablePart#doSave(org.eclipse.core.runtime.IProgressMonitor)
	 */
	public void doSave(IProgressMonitor monitor) {
		setSaved(true);
		super.doSave(monitor);
		this.documentModel.updateNow();
	}

	/**
	 * Updates the code folding of this editor.
	 * 
	 * @param rootNodes
	 *            The document tree that correspond to folds
	 * @param monitor
	 *            A progress monitor for the job doing the update
	 */
	public void updateCodeFolder(ArrayList rootNodes, IProgressMonitor monitor) {
		this.folder.update(rootNodes);
	}

	/**
	 * Triggers the model to be updated as soon as possible.
	 * 
	 * Used by drag'n'drop and copy paste actions of the outline.
	 */
	public void updateModelNow() {
		this.documentModel.updateNow();
	}

	/**
	 * Used by outline to determine whether drag'n'drop operations are
	 * permitted.
	 * 
	 * @return true if current model is dirty
	 */
	public boolean isModelDirty() {
		return this.documentModel.isDirty();
	}

	/**
	 * @see org.eclipse.ui.IWorkbenchPart#dispose()
	 */
	public void dispose() {
		super.dispose();
	}

	// B----------------------------------- mmaus

	/**
	 * 
	 * @return the fullOutline view.
	 */
	public TexOutlineTreeView getFullOutline() {
		return fullOutline;
	}

	/**
	 * register the full outline.
	 * 
	 * @param view
	 *            the view.
	 */
	public void registerFullOutline(TexOutlineTreeView view) {
		boolean projectChange = false;
		if (view == null || view.getEditor() == null) {
			projectChange = true;
		} else if (view.getEditor().getEditorInput() instanceof IFileEditorInput) {
			IFileEditorInput oldInput = (IFileEditorInput) view.getEditor().getEditorInput();
			IProject newProject = getProject();
			// Check whether the project changes
			if (!oldInput.getFile().getProject().equals(newProject))
				projectChange = true;
		} else
			projectChange = true;

		this.fullOutline = view;
		this.fullOutline.setEditor(this);
		if (projectChange) {
			// If the project changes we have to update the fulloutline
			this.fullOutline.projectChanged();
			this.documentModel.updateNow();
		}
	}

	/**
	 * unregister the full outline if the view is closed.
	 * 
	 * @param view
	 *            the view.
	 */
	public void unregisterFullOutline(TexOutlineTreeView view) {
		this.fullOutline = null;

	}

	public IDocument getTexDocument() {
		return this.getDocumentProvider().getDocument(getEditorInput());
	}

	// E----------------------------------- mmaus

	/**
	 * @return The project that belongs to the current file or null if it does
	 *         not belong to any project
	 */
	public IProject getProject() {
		IResource res = (IResource) getEditorInput().getAdapter(IResource.class);
		if (res == null)
			return null;
		else
			return res.getProject();
	}

	/**
	 * Initializes the key binding scopes of this editor.
	 */
	protected void initializeKeyBindingScopes() {
		setKeyBindingScopes(
				new String[] { "org.eclipse.ui.textEditorScope", "net.sourceforge.texlipse.texEditorScope" }); //$NON-NLS-1$
	}

	/*
	 * Made changes by Farhan
	 */

	@Override
	public Annotation gotoAnnotation(boolean forward) {
		// TODO Auto-generated method stub
		Annotation annotation = super.gotoAnnotation(forward);
		int lineNumber = 0;
		if (annotation instanceof MarkerAnnotation)
			lineNumber = MarkerUtilities.getLineNumber(((MarkerAnnotation) annotation).getMarker());
		if (annotation != null) {
			{
				try {
					TTSConversion.getDefault().stop(TTSProperties.SEND_DATA_NEXT);
					Thread.sleep(500);
					TTSConversion.getDefault().speak(annotation.getText() + " at line number " + lineNumber);
				} catch (IOException | InterruptedException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}

		}
		return annotation;
	}
	
	
	@Override
	protected void handleCursorPositionChanged() {
		//int doc_length = sourceDocument.getLength();
		lastCursorOffset = cursorOffset;
		lastLineNumber = lineNumber;
		int current_line_offset = 0;
		//current_line_offset = 0;
			cursorOffset=0;
		//String curosrText="";
		// TODO Auto-generated method stub
		super.handleCursorPositionChanged();			
		sourceDocument = getDocument();
		cursorOffset = getCursorOffset();	
		String pos = getCursorPosition();
		lineNumber = Integer.parseInt(pos.split(" : ")[0]);
		if(lastCursorOffset != cursorOffset || lastLineNumber != lineNumber){
			try {		
				current_line_offset = sourceDocument.getLineOffset(lineNumber - 1);
				int current_line_length = sourceDocument.getLineLength(lineNumber - 1);
				//current_line_length = sourceDocument.getLineLength(lineNumber - 1);
				currentLine = sourceDocument.get(current_line_offset, current_line_length);
				String currentLine_ = currentLine.replace(" ", "");	
				if (!currentLine_.equals("\r\n") && !currentLine_.equals("\n") && !currentLine_.isEmpty())
				{				 
						String tempChar = sourceDocument.get(current_line_offset, cursorOffset - 1);
						if(tempChar.trim().length() > 0) 
						{						
							nextCharacter = tempChar.toCharArray()[tempChar.length() - 1];												
							if(Character.isUpperCase(nextCharacter))
							{
								capsString = "Caps" + nextCharacter;
								capsLockStatus= true;
							}
							
							if (isCurserKeyPressed) 
							{
								if( nextCharacter == ' ' && !VbstCharAction.getDefault().isVerbosityChar())
								//if(!VbstCharAction.getDefault().isVerbosityChar())
								{
									if(!TexEditor.isStringNullOrWhiteSpace(typedWord)){
										speakCurserModeText(typedWord);								
										typedWord = "";
									}else{
										// Cursor navigation by word
//										String curosrText="";
//										cursorWordText= cursorNavigation(current_line_offset, current_line_length);												
//										speakCurserModeText(curosrText);
									}
								} 
								else if (VbstCharAction.getDefault().isVerbosityChar())
								{
									if (nextCharacter == ' ')
									{
										speakCurserModeText("space");
										typedWord = "";
									}
									else if (capsLockStatus)
									{
										speakCurserModeText(String.valueOf(capsString));
										capsLockStatus = false;
									}
									else 
									{
										speakCurserModeText(String.valueOf(nextCharacter));									
									}															
								}
								else{
//									String curosrText="";
//									cursorWordText= cursorNavigation(current_line_offset, current_line_length);	
//									if(curosrText.length() > 1){
//										speakCurserModeText(curosrText);
//										typedWord = "";
//									}
								}
								
								// ending cursor mode logic block
									typeWritingMode(cursorOffset);															
							}
							else 
							{						
							// Cursor navigation by word
//							String curosrText="";
//							cursorWordText= cursorNavigation(current_line_offset, current_line_length);												
//							speakCurserModeText(curosrText);
							}
						}
						else if (isCurserKeyPressed && tempLineNumber == lineNumber )
			            {                 						
							speakCurserModeText("space");
							typedWord = "";
			            }
						//new line logic
						else if (isCurserKeyPressed && tempLineNumber != lineNumber )
			            {
			                speakCurserModeText("empty line");
			                tempCursorOffset = 0;
			                typedWord = "";
			            }					
					} 
					else 
					{
						currentLine = "empty line";
//						if (isCurserKeyPressed && tempLineNumber != lineNumber )
//			            {                 
//			                speakCurserModeText("empty line"); 
//			                typedWord = "";
//			                tempCursorOffset=0; 
//			            }
					}
			} catch (BadLocationException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			if(lastLineNumber != lineNumber){
				speakCurserModeText(currentLine); 
			}
		}
        tempLineNumber = lineNumber;
	}

	private String cursorNavigation(int current_line_offset, int current_line_length) {
		//tempLineNumber = tempLineNumber - 1;
		if(lineNumber == tempLineNumber + 1 )
			tempCursorOffset = 1;
		else if(lineNumber == tempLineNumber - 1) {
			tempCursorOffset = current_line_length;
		}
		String jumper_word = "";
		if(cursorOffset > tempCursorOffset)
			jumper_word = sourceDocument.get().substring(current_line_offset + tempCursorOffset, current_line_offset + cursorOffset - 1);
		else if(cursorOffset < tempCursorOffset)
			jumper_word = sourceDocument.get().substring(current_line_offset + cursorOffset - 1, current_line_offset + tempCursorOffset);
		 
		System.out.println(jumper_word);
		//tempCursorOffset = (current_line_offset + cursorOffset) - (current_line_offset + tempCursorOffset);
	 	tempCursorOffset = tempCursorOffset - 1;
		return jumper_word;
	}

	private int getCursorOffset() {
		String pos = getCursorPosition();
		int cursorOffset = Integer.parseInt(pos.split(" : ")[1]);
		return cursorOffset;
	}

	private void typeWritingMode(int cursorOffset) {
		if(cursorOffset > tempCursorOffset)
			typedWord += nextCharacter;
		else if(cursorOffset < tempCursorOffset)
			if(typedWord.length() > 0)
			typedWord = typedWord.substring(0, typedWord.length() - 1);
		int lastTempCursorOffset = tempCursorOffset;
		tempCursorOffset = cursorOffset;
		tempCursorOffset-- ;
	}

	public int getCurrentLineNumber() {	
		return lineNumber;
	}
	public String getStartToCursor() {
		int current_line_offset=0;
		fromStartOfLineToCursor = "";
		String tempChar="";
		//IDocument document= getDocument();
		// cursorOffset = getCursorOffset();	
		try {
			 current_line_offset = sourceDocument.getLineOffset(lineNumber - 1);
			  tempChar = sourceDocument.get(current_line_offset, cursorOffset - 1);
		} catch (BadLocationException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		if(tempChar.trim().length() > 0) 
		{
			fromStartOfLineToCursor = currentLine.substring(0, cursorOffset - 1);
		}
		return fromStartOfLineToCursor;
	}
	
	public String getCurrentLine() {
		return currentLine;
	}

	public char getNextCharacter() {		
		return nextCharacter;
	}

	 protected String getForwardCursorPart() {
		
		int current_line_length=0;
		int current_line_offset=0;			
		//IDocument document = sourceDocument;//getDocument();		
		int doc_length = sourceDocument.getLength();
		//int cursorOffset = getCursorOffset();
		
		try {
			 current_line_offset = sourceDocument.getLineOffset(lineNumber - 1);
			 current_line_length = sourceDocument.getLineLength(lineNumber - 1);
		} catch (BadLocationException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
			if (doc_length != (cursorOffset - 1) + current_line_offset) 
			{		
				forwardCursorPart = currentLine.substring(cursorOffset - 1, current_line_length);
				String forwardCursorPart_ = forwardCursorPart.replace(" ", "");
				if(forwardCursorPart_.equals("\r\n") || forwardCursorPart_.equals("\n") || currentLine.equals("empty line")) {
				forwardCursorPart = "no further text";
				}
		   }
			else
			{
				forwardCursorPart= "no further text";
			}
				
		return forwardCursorPart;
	}

	public String getStartFromImmdiateWordkPart() {
		int current_line_length=0;
		//int cursorOffset = getCursorOffset();	
		//IDocument document = getDocument();		
		try {
			current_line_length = sourceDocument.getLineLength(lineNumber - 1);
		} catch (BadLocationException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		String currentLine_ = currentLine.replace(" ", "");
		if (!currentLine_.equals("\r\n") && !currentLine_.equals("\n") && !currentLine.equals("empty line")) {
			int temp = cursorOffset - 1;
			while (temp > 0 && currentLine.charAt(temp) != ' ' && currentLine.charAt(temp) != '{'
					&& currentLine.charAt(temp) != '\\')
				temp--;
			int offset = 1;
			if(temp  == 0)
				offset = 0;
			startFromImmediateWord = currentLine.substring(temp + offset, current_line_length);
			if(startFromImmediateWord.equals("\r\n") || startFromImmediateWord.equals("\n")) {
				startFromImmediateWord = "no further text";
				}
		}
		else 
			startFromImmediateWord="no further text";
		return startFromImmediateWord;
	}

	private IDocument getDocument() {
		ISourceViewer sourceViewer = getSourceViewer();
		IDocument document = sourceViewer.getDocument();
		return document;
	}	
	
	public String getForwardCursorPartTillEnd() 
	{
		int current_line_offset=0;		
		//int cursorOffset = getCursorOffset();	
		//IDocument document = getDocument();	
		int doc_length = sourceDocument.getLength();
		String doc_text = sourceDocument.get();
		try {
			 current_line_offset = sourceDocument.getLineOffset(lineNumber - 1);
		} catch (BadLocationException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		forwardCursorPartTillEnd = doc_text.substring(current_line_offset + cursorOffset - 1, doc_length);
		return forwardCursorPartTillEnd;
	}

	public String getStartFromImmdiateWordTillEnd() {
		int current_line_length= 0;
		String currentLine_ = currentLine.replace(" ", "");
		if (!currentLine_.equals("\r\n") && !currentLine_.equals("\n") ) {			
			//String pos = getCursorPosition();
			//IDocument document= getDocument();
			String doc_text = sourceDocument.get();
			int doc_length = sourceDocument.getLength();
			int current_line_offset=0;
			try {
				current_line_offset = sourceDocument.getLineOffset(lineNumber - 1);
				current_line_length = sourceDocument.getLineLength(lineNumber - 1);
			} catch (BadLocationException e) 
			{
				// TODO Auto-generated catch block
				e.printStackTrace();
			}			
			//int cursorOffset = Integer.parseInt(pos.split(" : ")[1]);	
			int	temp = cursorOffset - 1; 
			while (temp > 0 && currentLine.charAt(temp) != ' ' && currentLine.charAt(temp) != '{'
					&& currentLine.charAt(temp) != '\\')
				temp--;
			int offset = 1;
			if(temp  == 0)
				offset = 0;
			startFromImmediateWordTillEnd = doc_text.substring(current_line_offset + temp + offset, doc_length);
		}
		return startFromImmediateWordTillEnd;
	}
	
	public String getTypedWord() {
		return typedWord;
	}
	
	public void run(IAction action) {
		// TODO Auto-generated method stub

	}

	public void selectionChanged(IAction action, ISelection selection) {
		// TODO Auto-generated method stub
		
	}

	public void init(IAction action) {
		// TODO Auto-generated method stub

	}

	public void runWithEvent(IAction action, Event event) {
		String cursorText = "";
		switch (event.keyCode) {
		case 103:
			cursorText = String.valueOf(getCurrentLineNumber());
			break;
		case 104:
			cursorText = String.valueOf(getNextCharacter());
            if (isCurserKeyPressed)
            {
                isCurserKeyPressed= false;
                try {
                	TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_SPECIAL_CMND); //Ignore character mode
                	Thread.sleep(500);
					TTSConversion.getDefault().speak("Cursor mode disabled.");
				} catch (IOException | InterruptedException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
                cursorText="";
            }
            else
            {
                //cursorText = String.valueOf(getNextCharacter());
                isCurserKeyPressed= true;
                try {
                	TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_SPECIAL_CMND); 
                	Thread.sleep(500);
					TTSConversion.getDefault().speak("Cursor mode enabled.");
				} catch (IOException | InterruptedException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
                tempLineNumber=0;
                cursorText="";
            }
			break;
		case 105:
			cursorText = getForwardCursorPart();			
			break;
		case 106:
			cursorText = getStartFromImmdiateWordkPart();
			break;
		case 107: 
			cursorText = getStartToCursor();
			break;
		case 108 :
			cursorText = getCurrentLine();
			break;
		case 109:
			cursorText = getStartFromImmdiateWordTillEnd();
			break;
		}
		//Disable caps lock when shorkey pressed
		CapsLock.disableCapsLock();
		if (!(cursorText.length() <= 0) && !(cursorText.length() == 1)  && !cursorText.equals("space") && !cursorText.equals("empty line") && !cursorText.equals("no further text")
		          && !cursorText.equals("empty line") && !cursorText.contains("Caps")){
				cursorText += TTSProperties.SEND_DATA_EDITOR_CMND;
			}
		if(!cursorText.isEmpty())
			speakCurserModeText(cursorText);
			//send print (***TEditorComand**)
	}
 
	private void speakCurserModeText(String cursorText) {
		if (cursorText != null) {
			try {
				if (VbstCharAction.getDefault().isVerbosityChar() && cursorText.length() != 1) {
					TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_EDITOR_CMND); 
					Thread.sleep(500);
					TTSConversion.getDefault().speak(cursorText);
				} else {
					TTSConversion.getDefault().speak(cursorText);
				}
			} catch (Exception exp) {
				exp.printStackTrace();
			}
		}
	}
	
	public static boolean isStringNullOrWhiteSpace(String value) {
	    if (value == null) {
	        return true;
	    }

	    for (int i = 0; i < value.length(); i++) {
	        if (!Character.isWhitespace(value.charAt(i))) {
	            return false;
	        }
	    }

	    return true;
	}
	
	
}

