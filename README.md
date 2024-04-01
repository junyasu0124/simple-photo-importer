# Simple Photo Importer : A photoes and videos importer for Windows

Simple Photo Importer is a CLI tool that helps you to import photos and videos. With this tool, you can separate files by date, rename files, and copy them to a destination folder. You can also copy from multiple folders to multiple destination folders.

## Installation
Just download [THIS FILE](SimplePhotoImporter.exe) and put it in a folder you like, then you can use it.

## Usage

If you add the folder containing the .exe file to the PATH, you can use it from any folder by typing `SimplePhotoImporter` in the command prompt. Alternatively, even without adding it to the PATH, you can use it by specifying the full path to the .exe file.

### Interactive mode

```
SimplePhotoImporter
```
or
```
{directory path}\SimplePhotoImporter.exe
```
You can import easily in question-answer style.


### Command line mode

```
SimplePhotoImporter [app-option]
```

| app-option | Description |
| --- | --- |
| --help, -h | Show the usage of this tool |
| --version, -v | Show the version of this tool |

```
SimplePhotoImporter [--source-paths=...] [--dest-paths] ([--grouping-mode=...]) ([--file-name-format=...]) ([--directory-name-format=...]) ([--conflict-resolution=...]) ([options])
```

| Argument | Description |
| --- | --- |
| --source-paths=... | The paths to the folder you want to import from. You can enter multiple paths by separating them with a space (use double quotes if the path contains a space). If you want to exclude a directory, enter the path with a asterisk at the beginning. e.g. `"C:\Users\user\Pictures Folder" *"C:\Users\user\Pictures Folder\excluded"` |
| --dest-paths=... | The paths to the folder you want to import to. You can enter multiple paths by separating them with a space (use double quotes if the path contains a space). |
| --grouping-mode=... | The mode to for grouping files by directory. **1(\{space\})**: `\IMG_0001.jpg`, **2(y\\M\\d)**: `\yyyy\MM\dd\IMG_0001.jpg`, **3(y\\d)**: `\yyyy\dd\IMG_0001.jpg`, **4(d)**: `\dd\IMG_0001.jpg`  <Default: **1**> |
| --file-name-format=... | The format of the file name. Enter the format like `yyyyMMddHHmmss`. <Default: **\{OriginalFileName\}**> |
| --directory-name-format=... | The format of the directory name by year, month, and day. Enter the format like `yyyy\MM\dd`. You must specify {--grouping-mode=**2**: **three**, --grouping-mode=**3**: **two**, --grouping-mode=**4**: **one**} values separated by a backslash. |
| --conflict-resolution=... | The conflict resolution. **1**: Skip if the contents are the same, otherwise, add a number to the file name. **2**: Skip. **3**: Skip by directory. **4**: Add a number to the file name. **5**: Make a new directory having name with a number and add files to the directory. <Default\: **1**> |

| options | Description |
| --- | --- |
| --move | Move instead of copy |
| --lower-extension | Make the extension lower case |
| --upper-extension | Make the extension upper case |
| --custom-photo-extension | Add custom photo extension (e.g. `--custom-photo-extension=:remove`(<-Remove the default extensions)` .photo`) |
| --custom-video-extension | Add custom video extension (e.g. `--custom-video-extension=:remove`(<-Remove the default extensions)` .video`) |
| --date-info-priority | Change priority of the way to get shooting date time. **1**: Exif, **2**: Media created, **3**: Creation, **4**: Modified, **5**: Access  You must specify all values separated by a space. |
| --single-thread | Use a single thread |

## License

This software is released under the MIT License, see [LICENSE.txt](LICENSE.txt).

## Author

[junyasu0124](https://github.com/junyasu0124)

## Note

Please report any issues or requests on [GitHub's Issues page](https://github.com/junyasu0124/simple-photo-importer/issues). Thank you!
