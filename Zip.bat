cd "C:\Users\Flameo326\Documents\IDEs\Unity\Capstone\Assets"

DEL "..\EasyMarketingInUnity.zip"

::DEL "EasyMarketingInUnity\Plugins\log.txt"

::type NUL > "EasyMarketingInUnity\Plugins\log.txt"

7z a ..\EasyMarketingInUnity.zip EasyMarketingInUnity\* README.txt -r "-xr!*.meta" "-xr!*\Save\*" || PAUSE

pause