package net.sourceforge.texlipse.editor;

import org.eclipse.jface.action.IMenuManager;
import org.eclipse.jface.action.MenuManager;
import org.eclipse.jface.action.Separator;
import org.eclipse.ui.IWorkbenchActionConstants;
import org.eclipse.ui.editors.text.TextEditorActionContributor;

public class SpeachSettingsMenuContributor extends TextEditorActionContributor {

	public SpeachSettingsMenuContributor()
	{
		super();
	}
	
	@Override
    public void contributeToMenu(IMenuManager menuManager) {
    	super.contributeToMenu(menuManager);
    	
        //Add a new group to the navigation/goto menu
        IMenuManager gotoMenu = menuManager.findMenuUsingPath(IWorkbenchActionConstants.M_NAVIGATE+"/"+IWorkbenchActionConstants.GO_TO);
        if (gotoMenu != null) {
            gotoMenu.add(new Separator("additions2"));
        }
        
        IMenuManager editMenu = menuManager.findMenuUsingPath(IWorkbenchActionConstants.M_WINDOW);
        MenuManager manager = new MenuManager("TTS Settings");
        if (editMenu != null) {
            menuManager.insertBefore(IWorkbenchActionConstants.M_WINDOW, manager);
            MenuManager smallGreekMenu = new MenuManager("Greek lower case");
            MenuManager captialGreekMenu = new MenuManager("Greek upper case");
            MenuManager arrowsMenu = new MenuManager("Arrows");
            MenuManager compareMenu = new MenuManager("Compare symbols");
            MenuManager stdBinOpMenu = new MenuManager("Binary Operator");
            MenuManager stdBracesMenu = new MenuManager("Braces");
            MenuManager stdAccentsMenu = new MenuManager("Accents");
            manager.add(captialGreekMenu);
            manager.add(smallGreekMenu);
            manager.add(new Separator());
            manager.add(arrowsMenu);
            manager.add(compareMenu);
            manager.add(stdBinOpMenu);
            manager.add(stdBracesMenu);
            manager.add(new Separator());
            manager.add(stdAccentsMenu);               
        }
    }
	
	
	
	
}

