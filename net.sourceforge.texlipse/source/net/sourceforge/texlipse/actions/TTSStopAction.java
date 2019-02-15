package net.sourceforge.texlipse.actions;

import java.io.IOException;

import org.eclipse.jface.action.IAction;
import org.eclipse.jface.dialogs.MessageDialog;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.ui.IEditorActionDelegate;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IWorkbenchWindowActionDelegate;
import org.eclipse.ui.texteditor.ITextEditor;

import net.sourceforge.texlipse.TTSIntegration.CapsLock;
import net.sourceforge.texlipse.TTSIntegration.TTSConversion;
import net.sourceforge.texlipse.TTSIntegration.TTSProperties;

public class TTSStopAction implements IEditorActionDelegate {

	private ITextEditor textEditor;

	public TTSStopAction() {
	}

	public void run(IAction action) {
		if (textEditor == null)
			return;

		try {
			TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_STOP);
			//Disable caps lock when shorkey pressed
			CapsLock.disableCapsLock();
		} catch (Exception e) {
			// TODO: handle exception
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
