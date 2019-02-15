package net.sourceforge.texlipse.actions;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.net.SocketAddress;
import java.net.SocketTimeoutException;

import org.eclipse.jface.action.IAction;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.ui.IEditorActionDelegate;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IWorkbenchWindowActionDelegate;
import org.eclipse.ui.texteditor.ITextEditor;

import net.sourceforge.texlipse.TTSIntegration.CapsLock;
import net.sourceforge.texlipse.TTSIntegration.TTSConversion;
import net.sourceforge.texlipse.TTSIntegration.TTSProperties;
import net.sourceforge.texlipse.builder.AbstractProgramRunner;
import net.sourceforge.texlipse.editor.TexEditor;

public class TTSMathModeAction implements IEditorActionDelegate {

	private ITextEditor textEditor;
	private String EditorText;
	
	public TTSMathModeAction(){

	}
 
	public void run(IAction action) {
		if (textEditor == null)
			return;
		TexEditor editor = new TexEditor(); 
		EditorText = textEditor.getDocumentProvider().getDocument(textEditor.getEditorInput()).get();
		try { 
			if (EditorText.isEmpty()) {
				TTSConversion.getDefault().speak(TTSProperties.MSG_EMPTY_EDITOR);
			}  
			else if (editor.isSaved() && !AbstractProgramRunner.isErrors())
			{
				TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_MATHMODE);
				Thread.sleep(500);
				TTSConversion.getDefault().speak(EditorText);
			} 
			else 
			{ 
				TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_SPECIAL_CMND);
				Thread.sleep(500);
				TTSConversion.getDefault().speak(TTSProperties.MSG_SAVE_FILE_FIRST);
			}
			//Disable caps lock when shorkey pressed  
			CapsLock.disableCapsLock();
		} catch (IOException | InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	/**
	 * Selection in the workbench has been changed. We can change the state of
	 * the 'real' action here if we want, but this can only happen after the
	 * delegate has been created.
	 * 
	 * @see IWorkbenchWindowActionDelegate#selectionChanged
	 */
	public void selectionChanged(IAction action, ISelection selection) {

	}

	public void setActiveEditor(IAction action, IEditorPart targetEditor) {
		if (targetEditor instanceof ITextEditor) {
			this.textEditor = (ITextEditor) targetEditor;
		}
	}

}
