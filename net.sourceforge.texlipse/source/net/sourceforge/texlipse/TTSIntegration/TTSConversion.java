package net.sourceforge.texlipse.TTSIntegration;

import java.io.BufferedReader;
import java.io.BufferedWriter;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.ObjectInputStream;
import java.io.OutputStream;
import java.io.OutputStreamWriter;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.net.ServerSocket;
import java.net.Socket;
import java.net.SocketAddress;
import java.net.SocketTimeoutException;
import java.net.UnknownHostException;
import java.nio.charset.Charset;
import java.nio.file.Paths;
import java.util.stream.Stream;

import org.eclipse.core.resources.IFile;
import org.omg.CORBA.portable.InputStream;

import net.sourceforge.texlipse.actions.TTSOFFAction;
import net.sourceforge.texlipse.texparser.LatexParser;

public class TTSConversion {

	private Socket socket;
	private String testServerName = "127.0.0.1";
	private int port = 5000;
	private boolean TTSPowerFlag = true;
	public static String strLatexDocumentfromServer = "";
	private static TTSConversion instance;

	public TTSConversion() {
		try {
			// open a socket
			socket = openSocket(testServerName, port);
		} catch (Exception e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
			resetServer();
		}
	}

	public static TTSConversion getDefault() {
		if (instance == null)
			instance = new TTSConversion();
		return instance;
	}

	public void ReceivedDataFromServer() throws IOException {
		try {
			InputStreamReader inputStreamReader = new InputStreamReader(socket.getInputStream());
			BufferedReader bufferedReader = new BufferedReader(inputStreamReader);
			String mes = "";
			// while (bufferedReader.ready()) {
			// mes = bufferedReader.readLine();
			// System.err.println("Received by server "+mes);
			// if(strLatexDocumentfromServer=="")
			// {
			// strLatexDocumentfromServer=mes+"\n";
			// }
			// else
			// {
			// strLatexDocumentfromServer = strLatexDocumentfromServer +mes
			// +"\n";
			// }
			// }

			while ((mes = bufferedReader.readLine()) != null) {

				// System.err.println("Received by server TTS Conversion "+mes);
				if (strLatexDocumentfromServer == "") {

					strLatexDocumentfromServer = mes + "\n";
				} else {
					// strLatexDocumentfromServer =strLatexDocumentfromServer+
					// mes+"\n";
					if (mes.startsWith("\\begin{document}") || mes.contains("\\begin{document}")) {
						if (LatexParser.IsWarning == true) {
							strLatexDocumentfromServer = strLatexDocumentfromServer + mes + "\n";
							strLatexDocumentfromServer = strLatexDocumentfromServer + "\n"
									+ "\\footnote{\\underline{PDF is not accessible}}" + "\n";
							LatexParser.IsWarning = false;
						} else {
							strLatexDocumentfromServer = strLatexDocumentfromServer + mes + "\n";
						}
					} else {
						strLatexDocumentfromServer = strLatexDocumentfromServer + mes + "\n";
					}
				}
				if (mes.startsWith("\\end{document}") || mes.contains("\\end{document}")) {

					return;
				}
			}
			// inputStreamReader.close();
			// bufferedReader.reset();
			// bufferedReader.close();

		} catch (Exception ex) {
			System.out.println("Exception occured : TTS Conversion" + ex.getMessage());
			resetServer();
		}
	}

	public void speak(String message) throws IOException {
		// write-to, and read-from the socket.
		// in this case just write a simple command to a web server.
		// System.out.println(message+"\n");
		try {
			if (isTTSPowerFlag()) {
				// send data to server
				OutputStream os = socket.getOutputStream();
				socket.setTcpNoDelay(true);
				BufferedWriter bufferedWriter = new BufferedWriter(new OutputStreamWriter(os));
				bufferedWriter.write(message);
				// bufferedWriter.newLine();
				bufferedWriter.flush();

			}
		} catch (Exception e) {
			// TODO: handle exception
			e.getMessage();
			System.err.println("TTS Conversion \n" + e.getMessage());
			resetServer();
		}

	}

	public void stop(String message) throws IOException {
		// write-to, and read-from the socket.
		// in this case just write a simple command to a web server.
		if (isTTSPowerFlag()) {
			OutputStream os = socket.getOutputStream();
			BufferedWriter bufferedWriter = new BufferedWriter(new OutputStreamWriter(os));
			bufferedWriter.write(message);
			bufferedWriter.flush();
		}
	}

	/**
	 * Open a socket connection to the given server on the given port. This
	 * method currently sets the socket timeout value to 10 seconds. (A second
	 * version of this method could allow the user to specify this timeout.)
	 */
	public static Socket openSocket(String server, int port) throws Exception {
		Socket socket;

		// create a socket with a timeout
		try {
			InetAddress inteAddress = InetAddress.getByName(server);
			SocketAddress socketAddress = new InetSocketAddress(inteAddress, port);

			// create a socket
			socket = new Socket();

			// this method will block no more than timeout ms.
			int timeoutInMs = 10 * 1000; // 10 seconds
			socket.connect(socketAddress, timeoutInMs);

			return socket;
		} catch (SocketTimeoutException ste) {
			System.err.println("Timed out waiting for the socket.");
			ste.printStackTrace();
			throw ste;
		}
	}

	@Override
	protected void finalize() throws Throwable {
		// TODO Auto-generated method stub
		super.finalize();
		// close the socket, and we're done
		socket.close();
	}

	public boolean isTTSPowerFlag() {
		return TTSPowerFlag;
	}

	public void setTTSPowerFlag(boolean tTSPowerFlag) {
		TTSPowerFlag = tTSPowerFlag;
	}

	private void resetServer() {
		String path = Paths.get(".").toAbsolutePath().normalize().toString();
		try {
			Runtime.getRuntime().exec(TTSProperties.CMND_KILL_TTS);
			System.out.println("Resetting server");
			Thread.sleep(500);
			Runtime.getRuntime().exec(path + "/TTS/TTS_server_alap_alpha_v1.exe");
			socket = openSocket(testServerName, port); //// open a socket
		} catch (Exception e1) {
			// TODO Auto-generated catch block
			e1.printStackTrace();
		}
	}

}