cd "C:\Users\Flameo326\Documents\IDEs\Unity\Capstone\EasyMarketingInUnityExpress"

:: I expect to call this as Start.bat port_number
IF [%1]==[] (
     set PORT=3000
) ELSE (
    set PORT=%1
)

ECHO "Port = %PORT%"

npm start  || PAUSE

@pause