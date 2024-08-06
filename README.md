# FileCopyer

FileCopyer is a powerful, multi-threaded file copying tool designed to efficiently copy files and directories, handle errors, and generate comprehensive reports.

## Features

- **Multi-threaded copying**: Utilizes multiple threads to speed up the copying process.
- **File integrity verification**: Ensures copied files are identical to the source files using SHA-256 hashing.
- **Error handling**: Captures and logs errors during the copying process.
- **Temporary file handling**: Option to use temporary files during the copying process to prevent corruption.
- **Directory synchronization**: Identifies and copies new or modified files from the source to the destination.
- **User-friendly UI**: Easy-to-use Windows Forms application with real-time status updates.
- **Exit safety**: Ensures safe exit by waiting for ongoing operations to complete before closing the application.
- **Reports**: Generates detailed reports of the copying process, including errors and statistics.

## Installation

1. Clone the repository:
    ```sh
    git clone https://github.com/yourusername/FileCopyer.git
    ```
2. Open the solution in Visual Studio.
3. Build the project to restore the necessary packages and dependencies.

## Usage

1. Launch the application.
2. Use the settings form to specify source and destination directories.
3. Click "Start" to begin the copying process.
4. Monitor the progress and status in real-time via the UI.
5. Click "Stop" to pause or end the copying process.

## Configuration
To configure the application settings, follow these steps:

Open the settings form by clicking on the "Settings" button.
Add or remove source and destination directories.
Save the configurations.
Contributing

Contributions are welcome! Please follow these steps:

Fork the repository.
Create your feature branch (git checkout -b feature/your-feature).
Commit your changes (git commit -m 'Add some feature').
Push to the branch (git push origin feature/your-feature).
Open a pull request.
License
This project is licensed under the MIT License - see the LICENSE file for details.

Acknowledgements
Special thanks to all contributors and users who have provided feedback and suggestions.
Contact
If you have any questions, feel free to reach out:


**Email:** mthreat[dot]mob[at]gmail[dot]com

**GitHub:** mohammad-mirzaei-eng

Happy copying! 😊
