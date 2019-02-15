
package net.sourceforge.texlipse.actions;

import org.eclipse.jface.action.IAction;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.ui.IEditorActionDelegate;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IWorkbenchWindowActionDelegate;
import org.eclipse.ui.texteditor.ITextEditor;

import net.sourceforge.texlipse.TTSIntegration.CapsLock;
import net.sourceforge.texlipse.TTSIntegration.TTSConversion;
import net.sourceforge.texlipse.TTSIntegration.TTSProperties;

public class PDFAccessibility implements IEditorActionDelegate {

	private ITextEditor textEditor;
	private static boolean PDFAccessibilityModeOn = false;
	private static boolean PDFFullyAccessible = false;
	private static PDFAccessibility instance;

	public PDFAccessibility() {

	}

	public static PDFAccessibility getDefault() {
		if (instance == null)
			instance = new PDFAccessibility();
		return instance;
	}

	public void run(IAction action) { 
		if (textEditor == null)
			return;
		togglePDFAccessiblityMode();
		//Disable caps lock when shorkey pressed
		CapsLock.disableCapsLock();
	}

	private void togglePDFAccessiblityMode() {
		try {
			if(isPDFAccessibilityModeOn()){
				TTSConversion.getDefault().speak(TTSProperties.ALERT_PDF_ACCESSIBLITY_MODE_OFF);
				setPDFAccessibilityModeOn(false);
//				TTSConversion.getDefault().speak("**(LatexDocumentFlagOFF)**");
			} 
			else{
				TTSConversion.getDefault().speak(TTSProperties.ALERT_PDF_ACCESSIBLITY_MODE_ON);
				setPDFAccessibilityModeOn(true);
//				TTSConversion.getDefault().speak("**(LatexDocumentFlagON)**");
//				TTSConversion.getDefault().speak(TTSProperties.ALERT_PDF_ACCESSIBLITY_MODE_ON);
				//TTSConversion.getDefault().speak("**(LatexDocument)**"); 
			}
		}
		catch(Exception ex) {
			ex.printStackTrace();
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
	/**
	 * @return the pDFAccessibilityModeOn
	 */
	public static boolean isPDFAccessibilityModeOn() {
		return PDFAccessibilityModeOn; 
	}

	/**
	 * @param pDFAccessibilityModeOn the pDFAccessibilityModeOn to set
	 */
	public static void setPDFAccessibilityModeOn(boolean pDFAccessibilityModeOn) {
		PDFAccessibilityModeOn = pDFAccessibilityModeOn;
	}

//	public static boolean isPDFFullyAccessible() {
//		return PDFFullyAccessible;
//	}

//	public static void setPDFFullyAccessible(boolean pDFFullyAccessible) {
//		PDFFullyAccessible = pDFFullyAccessible;
//	}

}