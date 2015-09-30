from subprocess import call
def ScanBarcode( grid):
    "scan barcode for certain grid"
    scanResult = True;
    sBinFolder = "C:\\BarcodeGuard\\bins\\";
    while True:
        call([sBinFolder + "SimulatePosID.exe",str(grid)]);
        call([sBinFolder + "Notifier.exe",str(grid)]); #notify main program to read barcode of grid x
        call([sBinFolder + "FeedMe.exe","FeedMe"]);
        resultFile = open("c:\\BarcodeGuard\\Output\\result.txt");
        result = resultFile.read();
        resultFile.close();
        if result == "True":
            break;

def main():
    sBinFolder = "C:\\BarcodeGuard\\bins\\";
    call([sBinFolder + "FeedMe.exe","FeedMe"]);
    f = open("C:\\BarcodeGuard\\Output\\gridsCount.txt");
    gridNum = f.read();
    f.close();
    startGrid = 5;
    endGrid = startGrid + int(gridNum);
    for grid in range(startGrid,endGrid):
        ScanBarcode(grid);

main();