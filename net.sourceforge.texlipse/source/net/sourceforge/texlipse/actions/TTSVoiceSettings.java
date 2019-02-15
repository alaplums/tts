package net.sourceforge.texlipse.actions;

import java.io.IOException;

import javax.sound.midi.Synthesizer;

import org.eclipse.core.commands.AbstractHandler;
import org.eclipse.core.commands.ExecutionEvent;
import org.eclipse.core.commands.ExecutionException;
import org.eclipse.jface.action.ContributionItem;
import org.eclipse.jface.action.IAction;
import org.eclipse.jface.viewers.ISelection;
import org.eclipse.ui.IEditorActionDelegate;
import org.eclipse.ui.IEditorPart;
import org.eclipse.ui.IWorkbenchWindowActionDelegate;
import org.eclipse.ui.texteditor.ITextEditor;

import org.eclipse.jface.action.ContributionItem;
import org.eclipse.swt.SWT;
import org.eclipse.swt.events.SelectionAdapter;
import org.eclipse.swt.events.SelectionEvent;
import org.eclipse.swt.widgets.Menu;
import org.eclipse.swt.widgets.MenuItem;

import net.sourceforge.texlipse.TTSIntegration.CapsLock;
import net.sourceforge.texlipse.TTSIntegration.TTSConversion;
import net.sourceforge.texlipse.TTSIntegration.TTSProperties;

public class TTSVoiceSettings extends AbstractHandler {

	private static final String VOICE_SEL = "net.sourceforge.texlipse.dropdown.voice";

	//Synthesizer synth = new Synthesizer(); 
	public TTSVoiceSettings() {
		
	}

	public Object execute(ExecutionEvent event) throws ExecutionException {
	    String selectedVoice = event.getParameter(VOICE_SEL);
	    if (selectedVoice != null) {
	    	try {
				TTSConversion.getDefault().speak(TTSProperties.SEND_DATA_CHANGE_VOICE);
				Thread.sleep(500);
				TTSConversion.getDefault().speak(selectedVoice);
			} catch (IOException | InterruptedException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
	    }
	    return null;
	  }
} 
