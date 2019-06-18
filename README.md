# Accessible LaTex based Authoring and Presentation for Persons with Vision Impairment (ALAP - *Text to Speech Engine*)
ALAP is an accessible extension of the TeXlipse plugin for Eclipse, which provides support for LaTeX projects. ALAP comes with a fully integrated TTS(text-to-speech) engine which is highly customizable. Below is the list of main features offered:

* Enable/Disable TTS
* Set different Verbosity Levels (Word/Character)
* Increase/Decrease Speed
* Pause/Resume TTS
* Change Voice
* Comprehensive Set of Shortcut Keys
* ...and many more

"TTS" is developed using Microsoft's Speech API. This engine understands LaTeX and supports speech based creation and editing of mathematical documents. It also supports accessible debugging by assisting users with navigation through errors and narrating them to the users.

# History
ALAP version 1.0 is brought to you by the ALAP-team as a research project developed in the KADE Lab at Lahore University of Management Sciences. The ALAP-team includes : Ahtsham Manzoor, Safa Arooj, Shaban Zulfiqar, Omer Hayat, Dr. Suleman Shahid and Dr. Asim Karim.

# Technical Details
"TTS" is developed using Microsoft's Speech API (Version=4.0.0.0). 

The open soruce library [MathML](https://www.w3.org/Math/whatIsMathML.html "MathML") is used to generate XML tree of error free LaTeX math equations which is further used for conversion to descprtive narration. 

After converting the LaTeX mathematical commands to their appropriate description, to further refine the narration [Detex Libary](http://urchin.earth.li/~tomford/detex/ "Detex for Windows") is used for wiping the document of LaTeX tags. 

Please have a look at our [contribution guide](https://github.com/alaplums/TeXlipse/blob/master/CONTRIBUTING.md "Contribution Guide") if you would like to help out.

### Run TTS
TTS employs two spearate lookup files for converting LaTeX commands and symbols to their english description. These files can be found at [LookUp Files](https://github.com/alaplums/tts/tree/master/LookupTables) and must be present in your *bin/* folder for successful run of this program.  

# Resources
* [Official ALAP Website](https://alap.lums.edu.pk/ "ALAP")
* [Detailed User Guide](https://alap.lums.edu.pk/wp-content/uploads/2018/11/ALAP_UserGuide.pdf "ALAP User Guide")
