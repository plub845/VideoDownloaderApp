# 🎬 VideoDownloaderApp

A Windows-based application designed for downloading high-quality videos and audio from various public streaming media and standard network protocols. Powered by `yt-dlp` and `ffmpeg` for maximum performance and stability.

แอปพลิเคชันสำหรับระบบปฏิบัติการ Windows สำหรับดาวน์โหลดวิดีโอและไฟล์เสียงจากลิงก์สตรีมมิ่งและเครือข่ายสาธารณะ พัฒนาขึ้นโดยใช้ `yt-dlp` และ `ffmpeg` เพื่อประสิทธิภาพและความเสถียรในการทำงาน

<p align="left">
  <img src="https://img.shields.io/badge/License-GPLv3-blue.svg?style=for-the-badge" alt="GPLv3 License">
  <img src="https://img.shields.io/badge/Platform-Windows%20%7C%20Linux-0078D6.svg?style=for-the-badge&logo=windows" alt="Windows & Linux">
  <img src="https://img.shields.io/badge/Language-C%23-239120.svg?style=for-the-badge&logo=c-sharp" alt="C#">
  <img src="https://img.shields.io/badge/Status-Active-brightgreen.svg?style=for-the-badge" alt="Status Active">
</p>

---

## ⚡ Quick Install / ติดตั้งแบบรวดเร็ว

### 🪟 วิธีแนะนำ: ติดตั้งผ่าน PowerShell (Windows)

เปิด **PowerShell** หรือ **Windows Terminal** แล้วคัดลอกคำสั่งนี้ไปรันได้เลย:

```powershell
irm "https://raw.githubusercontent.com/plub845/VideoDownloaderApp/main/VideoDownloaderApp/main/install.ps1" -OutFile "$env:TEMP\VideoDownloaderApp-install.ps1"; powershell -ExecutionPolicy Bypass -File "$env:TEMP\VideoDownloaderApp-install.ps1"
```

สคริปต์นี้จะทำหน้าที่เป็นตัวช่วยดาวน์โหลดและเปิดตัวติดตั้งหลักจาก GitHub Release โดยอัตโนมัติ

**วิธีเปิด PowerShell**
1. กดปุ่ม **Start**
2. พิมพ์คำว่า **PowerShell**
3. คลิกขวาแล้วเลือก **Run as administrator**
4. วางคำสั่งด้านบน
5. กด **Enter**

> หมายเหตุ: หาก Windows หรือ SmartScreen แสดงคำเตือน ให้ตรวจสอบว่าดาวน์โหลดจาก repository นี้เท่านั้น และกดยืนยันเฉพาะเมื่อเชื่อถือแหล่งที่มา

### 🐧 วิธีติดตั้งบน Linux

เปิด **Terminal** แล้วคัดลอกคำสั่งนี้ไปรัน:

```bash
wget -O install.sh "https://github.com/plub845/VideoDownloaderApp/releases/download/v1.1.0/install.sh" && chmod +x install.sh && ./install.sh
```

---

## ⚙️ การติดตั้ง .NET SDK แบบแมนนวล (สำหรับผู้ที่ต้องการติดตั้งเอง)

หากตัวติดตั้งทำงานผิดพลาด หรือต้องการติดตั้งไฟล์ที่จำเป็นด้วยตนเอง คุณสามารถดาวน์โหลด `.NET SDK` ได้โดยตรงจากหน้า Release:

1. ไปที่หน้า Release v1.1.0:
```txt
https://github.com/plub845/VideoDownloaderApp/releases/tag/v1.1.0
```
2. ดาวน์โหลดไฟล์ `dotnet-sdk-8.0.422-win-x64.exe`
3. ดับเบิลคลิกเพื่อติดตั้งตามขั้นตอนปกติจนเสร็จสิ้น
4. หลังจากนั้นจึงเปิดตัวติดตั้งแอปพลิเคชัน VideoDownloaderApp

---

### ดาวน์โหลดไฟล์สคริปต์โดยตรง

สามารถเปิดไฟล์ `install.ps1` ได้ที่นี่:

```txt
https://github.com/plub845/VideoDownloaderApp/blob/main/VideoDownloaderApp/main/install.ps1
```

หรือดาวน์โหลดแบบ Raw:

```txt
https://raw.githubusercontent.com/plub845/VideoDownloaderApp/main/VideoDownloaderApp/main/install.ps1
```

### ตัวติดตั้งแบบ EXE

หากต้องการดาวน์โหลดตัวติดตั้งโดยตรง ให้ไปที่หน้า Releases:

```txt
https://github.com/plub845/VideoDownloaderApp/releases
```

---

## 🇺🇸 English Documentation

### 🎯 Key Features

| Features                    | Description                                                                 |
| :-------------------------- | :-------------------------------------------------------------------------- |
| 📦 **Media Downloads**      | Supports video extraction in `.mp4` and audio extraction in `.mp3` formats. |
| 🔗 **Advanced URL Parsing** | Handles complex streaming URLs, parameters, playlists, and media links.     |
| 📂 **Custom Output Paths**  | Allows users to freely specify target directories for saved media files.    |
| 📊 **Progress Monitoring**  | Includes a real-time progress bar to display active download status.        |
| ⚙️ **Custom Options**       | Supports additional command-line parameters for advanced `yt-dlp` control.  |
| 🧪 **Experimental Support** | Supports `.m3u8` and standard live streaming URL processing in beta stage.  |

### 🛠️ How to Use

Streaming URL ──> [ Paste Link ] ──> [ Select MP4/MP3 ] ──> [ Choose Folder ] ──> 📥 Success

1. Install the app using the **Quick Install** command above, or download the installer from the Releases page.
2. Open **VideoDownloaderApp**.
3. Copy a valid media URL or `.m3u8` link and paste it into the input field.
4. Select your preferred output format: **MP4** or **MP3**.
5. Choose the destination folder.
6. Click **Download** and wait for the process to complete.

### ⚠️ Technical Considerations & Notes

* **Custom Options Field:** Reserved for passing additional flags to `yt-dlp`, such as `--playlist-items 1-5`.
* **Dependencies:** This application uses `yt-dlp` and `ffmpeg` as its download and conversion engines.
* **Windows Warning:** Because this is an independent unsigned project, Windows or Microsoft Edge may show a warning on first download.

### 💻 Compilation Guide

For developers who want to modify or build the binary manually:

1. Open the project root directory using **Microsoft Visual Studio**.
2. Open the main solution file: `VideoDownloaderApp.sln`.
3. Switch the build configuration to **Release**.
4. Run **Build Solution**.
5. The compiled executable will be generated inside the build output directory.

---

## 🇹🇭 เอกสารภาษาไทย

### 🎯 คุณสมบัติการใช้งาน

| ฟังก์ชันหลัก                   | รายละเอียดการทำงาน                                        |
| :----------------------------- | :-------------------------------------------------------- |
| 📦 **ดาวน์โหลดวิดีโอ/เสียง**   | รองรับการดาวน์โหลดวิดีโอเป็น `.mp4` และเสียงเป็น `.mp3`   |
| 🔗 **รองรับลิงก์ซับซ้อน**      | รองรับ URL ที่มีพารามิเตอร์ เพลย์ลิสต์ และลิงก์สตรีมมิ่ง  |
| 📂 **เลือกโฟลเดอร์ปลายทางได้** | ผู้ใช้สามารถกำหนดตำแหน่งบันทึกไฟล์ได้เอง                  |
| 📊 **แสดงความคืบหน้า**         | มี Progress Bar และ Log สำหรับดูสถานะการทำงาน             |
| ⚙️ **คำสั่งขั้นสูง**           | รองรับการใส่คำสั่งเสริมของ `yt-dlp` ในช่อง Custom Options |
| 🧪 **รองรับ m3u8 แบบทดลอง**    | รองรับลิงก์สตรีมมิ่ง `.m3u8` ในระดับทดสอบ                 |

### 🛠️ ขั้นตอนการใช้งาน

ลิงก์วิดีโอ ──> [ วางลิงก์ ] ──> [ เลือก MP4/MP3 ] ──> [ เลือกโฟลเดอร์ ] ──> 📥 ดาวน์โหลดสำเร็จ

1. ติดตั้งโปรแกรมด้วยคำสั่งในหัวข้อ **Quick Install** ด้านบน หรือดาวน์โหลดตัวติดตั้งจากหน้า Releases
2. เปิดโปรแกรม **VideoDownloaderApp**
3. วางลิงก์วิดีโอ ลิงก์เสียง หรือลิงก์ `.m3u8`
4. เลือกรูปแบบไฟล์ที่ต้องการ: **MP4** หรือ **MP3**
5. เลือกโฟลเดอร์ปลายทาง
6. กดปุ่ม **Download** แล้วรอจนเสร็จ

### ⚠️ ข้อควรทราบ

* ช่อง **Custom Options** ใช้สำหรับใส่คำสั่งเสริมของ `yt-dlp` เช่น `--playlist-items 1-5`
* โปรแกรมนี้ใช้ `yt-dlp` และ `ffmpeg` เป็น engine หลัก
* เนื่องจากโปรเจกต์นี้เป็นโปรแกรมอิสระและยังไม่มีลายเซ็นดิจิทัล Windows หรือ Microsoft Edge อาจแสดงคำเตือนในครั้งแรกที่ดาวน์โหลด

### 💻 ขั้นตอนการ Build จากซอร์สโค้ด

สำหรับผู้พัฒนาที่ต้องการนำซอร์สโค้ดไปพัฒนาต่อ:

1. เปิดโฟลเดอร์โปรเจกต์ด้วย **Microsoft Visual Studio**
2. เปิดไฟล์ `VideoDownloaderApp.sln`
3. เปลี่ยนโหมด Build เป็น **Release**
4. กด **Build Solution**
5. ไฟล์ `.exe` จะถูกสร้างในโฟลเดอร์ผลลัพธ์ของการ Build

---

## 🤝 Acknowledgements / กิตติกรรมประกาศ

This project relies on powerful open-source utilities. Sincere gratitude to the developers and communities behind:

โปรเจกต์นี้ใช้เครื่องมือโอเพนซอร์สประสิทธิภาพสูง ขอขอบคุณผู้พัฒนาและชุมชนของเครื่องมือต่อไปนี้:

* **[yt-dlp](https://github.com/yt-dlp/yt-dlp)** — Advanced command-line media downloader utility.
* **[FFmpeg](https://ffmpeg.org/)** — Cross-platform solution to record, convert, and stream audio and video.

---

## ⚖️ Disclaimer / ข้อปฏิเสธความรับผิดชอบ

> This application is developed strictly for **educational purposes** and **personal use** only. The developer does not condone, support, or encourage any form of copyright infringement.
>
> By using this software, you agree to the following terms:
>
> * **No Commercial Use:** You must not use this tool to download copyrighted material for commercial profit or unauthorized distribution.
> * **Respect Content Creators:** You are solely responsible for compliance with the terms of service of the respective hosting platforms and local copyright laws.
> * **User Liability:** The developer shall not be held liable or responsible for any misuse, copyright claims, or damages resulting from the use of this application.
>
> แอปพลิเคชันนี้พัฒนาขึ้นเพื่อ **วัตถุประสงค์ทางการศึกษา** และ **การใช้งานส่วนบุคคล** เท่านั้น ผู้พัฒนาไม่สนับสนุนการละเมิดลิขสิทธิ์ทุกรูปแบบ ผู้ใช้งานต้องเป็นผู้รับผิดชอบต่อกฎหมายลิขสิทธิ์และเงื่อนไขของแพลตฟอร์มปลายทางด้วยตนเอง หากเกิดการนำไปใช้ในทางที่ผิดกฎหมาย ทางผู้พัฒนาจะไม่รับผิดชอบใด ๆ ทั้งสิ้น

---

## 📜 License / สัญญาอนุญาต

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)**.

โปรเจกต์นี้อยู่ภายใต้สัญญาอนุญาต **GNU General Public License v3.0 (GPL-3.0)**

### 🟩 Permissions / สิทธิ์ในการใช้งาน

Commercial use, modification, and distribution are permitted under the GPL-3.0 license.

อนุญาตให้นำซอร์สโค้ดไปใช้งาน ดัดแปลง แก้ไข หรือแจกจ่ายต่อได้ตามเงื่อนไขของ GPL-3.0

### 🟧 Conditions / เงื่อนไขผูกพัน

Any derivative works distributed at scale must disclose their source code under the same GPL-3.0 license and include proper attribution to the original author.

หากมีการนำซอร์สโค้ดนี้ไปดัดแปลง แก้ไข หรือแจกจ่ายต่อ ผู้ดำเนินการต้องเปิดเผยซอร์สโค้ดภายใต้สัญญาอนุญาตเดิม และต้องคงเครดิตของผู้พัฒนาต้นฉบับไว้
