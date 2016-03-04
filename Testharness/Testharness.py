from subprocess import call
sOutputFolder = "F:\\Projects\\BarcodeGuard.git\\trunk\\GuarderBuffy\\bin\\Output\\"
def ScanBarcode( grid):
    "scan barcode for certain grid"
    scanResult = True;
    sBinFolder = "C:\\BarcodeGuard\\bins\\";
    while True:
        call([sBinFolder + "SimulatePosID.exe",str(grid)]);
        call([sBinFolder + "Notifier.exe",str(grid)]); #notify main program to read barcode of grid x
        call([sBinFolder + "FeedMe.exe","FeedMe"]);
        resultFile = open(sOutputFolder + "result.txt");
        result = resultFile.read();
        resultFile.close();
        if result == "True":
            break;
        retryOrIgnoreFile = open(sOutputFolder + "retryOrIgnore.txt");
        content = retryOrIgnoreFile.read();
        retryOrIgnoreFile.close();
        if content == "Ignore":
            break;

def main():
    sBinFolder = "C:\\BarcodeGuard\\bins\\";
    call([sBinFolder + "FeedMe.exe","FeedMe"]);
    f = open(sOutputFolder + "gridsCount.txt");
    gridNum = f.read();
    f.close();
    startGrid = 7;
    endGrid = startGrid + int(gridNum);
    for grid in range(startGrid,endGrid):
        ScanBarcode(grid);

main();
