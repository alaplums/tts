
package net.sourceforge.texlipse.actions;

import java.io.IOException;

import org.eclipse.jface.action.IAction;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.ui.IEditorActionDelegate;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IWorkbenchWindowActionDelegate;
import org.eclipse.ui.texteditor.ITextEditor;

import net.sourceforge.texlipse.TTSIntegration.CapsLock;
import net.sourceforge.texlipse.TTSIntegration.TTSConversion;
import net.sourceforge.texlipse.TTSIntegration.TTSProperties;

public class VbstCharAction implements IEditorActionDelegate {

	private ITextEditor textEditor;
	private static boolean isVerbosityChar = false;

	private static VbstCharAction instance;

	public VbstCharAction() {

	}

	public static VbstCharAction getDefault() {
		if (instance == null)
			instance = new VbstCharAction();
		return instance;
	}

	public void run(IAction action) {
		if (textEditor == null)
			return;

		setVerbosityChar(true); 
		
		try {
			TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_CHAR_VRBSTY);
			Thread.sleep(500);
			TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_SPECIAL_CMND);
			Thread.sleep(100);
			TTSConversion.getDefault().speak(TTSProperties.MSG_VBRST_CHNG_CHR);
		} catch (IOException | InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		//Disable caps lock when shorkey pressed
		CapsLock.disableCapsLock();
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

	public boolean isVerbosityChar() {
		return isVerbosityChar;
	}

	public void setVerbosityChar(boolean isVerbosityChar) {
		this.isVerbosityChar = isVerbosityChar;
		
	}
}