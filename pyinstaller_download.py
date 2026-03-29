import urllib.request
import zipfile
print('Downloading...')
urllib.request.urlretrieve('https://github.com/pyinstaller/pyinstaller/archive/refs/tags/v6.5.0.zip', 'pyi.zip')
print('Extracting...')
zipfile.ZipFile('pyi.zip').extractall()
print('Done!')
