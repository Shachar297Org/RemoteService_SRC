if "%USERNAME%" == "serviceuser" (
  start "" "D:\Program Files\Lumenis\ConnectWise_POC\ServiceToken\ServiceToken.exe" r 10001
) else (
 
  runas /user:ServiceUser /savecred  "D:\Program Files\Lumenis\ConnectWise_POC\ServiceToken\ServiceToken.exe" r 10001
)