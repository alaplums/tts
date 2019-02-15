package net.sourceforge.texlipse.TTSIntegration;

import java.awt.Toolkit;
import java.awt.event.KeyEvent;

public class  CapsLock {
	public static void disableCapsLock()
	{
		try {
			Thread.sleep(1000);
			if(Toolkit.getDefaultToolkit().getLockingKeyState(KeyEvent.VK_CAPS_LOCK))
				Toolkit.getDefaultToolkit().setLockingKeyState(KeyEvent.VK_CAPS_LOCK, false);
		} catch (InterruptedException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
	}

}
