
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

public class VbstWordAction implements IEditorActionDelegate {

	private ITextEditor textEditor;

	public VbstWordAction() {
	}

	public void run(IAction action) {
		if (textEditor == null)
			return;
		VbstCharAction.getDefault().setVerbosityChar(false);
		try {
			TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_WORD_VRBSTY);
			Thread.sleep(500);
//			TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_SPECIAL_CMND);
//			Thread.sleep(100);
			TTSConversion.getDefault().speak(TTSProperties.MSG_VBRST_CHNG_WORD);
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

