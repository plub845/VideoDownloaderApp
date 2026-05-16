# 🎬 VideoDownloaderApp

A Windows-based application designed for downloading high-quality videos and audio from various public streaming media and standard network protocols. Powered by `yt-dlp` and `ffmpeg` for maximum performance and stability.

แอปพลิเคชันสำหรับระบบปฏิบัติการ Windows ในการดาวน์โหลดวิดีโอและไฟล์เสียงจากลิงก์สตรีมมิ่งและเครือข่ายสาธารณะ พัฒนาขึ้นโดยใช้เทคโนโลยี `yt-dlp` และ `ffmpeg` เพื่อประสิทธิภาพและความเสถียรในการทำงาน

<p align="left">
  <img src="https://img.shields.io/badge/License-GPLv3-blue.svg?style=for-the-badge" alt="GPLv3 License">
  <img src="https://img.shields.io/badge/Platform-Windows-0078D6.svg?style=for-the-badge&logo=windows" alt="Windows">
  <img src="https://img.shields.io/badge/Language-C%23-239120.svg?style=for-the-badge&logo=c-sharp" alt="C#">
  <img src="https://img.shields.io/badge/Status-Active-brightgreen.svg?style=for-the-badge" alt="Status Active">
</p>

---

## 🇺🇸 English Documentation

### 🎯 Key Features

| Features | Description |
| :--- | :--- |
| 📦 **Media Downloads** | Supports video extraction in `.mp4` and audio extraction in `.mp3` formats. |
| 🔗 **Advanced URL Parsing** | Fully handles complex streaming URLs, parameters, and media playlists. |
| 📂 **Custom Output Paths** | Allows users to freely specify target directories for saved media files. |
| 📊 **Progress Monitoring** | Includes a real-time progress bar to display active download status. |
| ⚙️ **Custom Options** | Supports additional command-line parameters (Custom Options) for granular control. |
| 🧪 **Experimental Support** | Supports `.m3u8` and standard live streaming URL processing (Beta stage). |

### 🛠️ How to Use

Streaming URL ──> [ Paste Link ] ──> [ Select MP4/MP3 ] ──> [ Choose Folder ] ──> 📥 Success


1. Navigate to the directory: `VideoDownloaderApp-main\VideoDownloaderApp\main`
2. Launch the main executable: **`VideoDownloaderApp.exe`** *(Note: All necessary dependencies like `yt-dlp.exe` and `ffmpeg.exe` are already pre-bundled in this directory and ready to use).*
3. Copy a valid media network URL or `.m3u8` link and paste it into the provided input field.
4. Select your preferred output format: **MP4** or **MP3**.
5. Set the target destination folder.
6. Click the **Download** button and wait for the process to complete.

### ⚠️ Technical Considerations & Notes

* **Custom Options Field:** Reserved for passing additional flags to yt-dlp (e.g., use `--playlist-items 1-5` to download specific range items if supported by the host).
* **UI Constraints:** Please avoid typing or inputting data into the area below the blue text indicator, as this region is strictly reserved for internal application status logs.

### 💻 Compilation Guide (Building from Source)

For developers who wish to modify or build the binary manually:

1. Open the project root directory using **Microsoft Visual Studio**.
2. Open the main solution file (`.sln`).
3. Switch the build configuration to **`Release`** via the top toolbar.
4. Execute the **Build Solution** command. The compiled executable (`.exe`) will be generated inside the `bin\Release` folder.

---

## 🇹🇭 เอกสารภาษาไทย

### 🎯 คุณสมบัติการใช้งาน

| ฟังก์ชันหลัก | รายละเอียดการทำงาน |
| :--- | :--- |
| 📦 **การดาวน์โหลดรูปแบบสื่อ** | รองรับการดาวน์โหลดไฟล์วิดีโอในรูปแบบ `.mp4` และไฟล์เสียงในรูปแบบ `.mp3` |
| 🔗 **การประมวลผลลิงก์** | รองรับโครงสร้าง URL ของลิงก์สตรีมมิ่งสาธารณะที่มีพารามิเตอร์ซับซ้อน รวมถึงระบบเพลย์ลิสต์ข้อมูล |
| 📂 **การจัดการเส้นทางไฟล์** | ผู้ใช้สามารถกำหนดโฟลเดอร์ปลายทางในการบันทึกข้อมูลได้อย่างอิสระ |
| 📊 **การแสดงผลสถานะ** | มีระบบ Progress Bar เพื่อแสดงความคืบหน้าของกระบวนการดาวน์โหลดในขณะทำงาน |
| ⚙️ **การกำหนดค่าขั้นสูง** | รองรับการระบุคำสั่งพิเศษเพิ่มเติม (Custom Options) สำหรับปรับแต่งการทำงานของเอนจินหลัก |
| 🧪 **ฟังก์ชันทดลอง** | รองรับการประมวลผลลิงก์สตรีมมิ่งประเภทเครือข่าย `.m3u8` (สถานะเวอร์ชันทดสอบ) |

### 🛠️ ขั้นตอนการใช้งาน

ลิงก์เครือข่ายสตรีมมิ่ง ──> [ วางลิงก์ ] ──> [ เลือก MP4/MP3 ] ──> [ กำหนดโฟลเดอร์ ] ──> 📥 ดาวน์โหลดสำเร็จ


1. เข้าไปที่ไดเรกทอรีโปรเจกต์: `VideoDownloaderApp-main\VideoDownloaderApp\main`
2. เปิดใช้งานไฟล์ระบบหลักชื่อ **`VideoDownloaderApp.exe`** *(หมายเหตุ: ภายในไดเรกทอรีนี้ได้จัดเตรียมไฟล์พึ่งพาสำเร็จรูปอย่าง `yt-dlp.exe` และ `ffmpeg.exe` พร้อมใช้งานไว้เรียบร้อยแล้ว)*
3. คัดลอกลิงก์สื่อสาธารณะ หรือลิงก์โครงสร้าง `.m3u8` ที่ต้องการ นำมาวางในช่องระบุข้อมูล
4. เลือกรูปแบบไฟล์ที่ต้องการจัดเก็บระหว่าง **MP4** หรือ **MP3**
5. กำหนดเส้นทางโฟลเดอร์ปลายทางที่ต้องการบันทึกไฟล์ข้อมูล
6. คลิกปุ่ม **Download** เพื่อเริ่มกระบวนการ และรอจนกว่าแถบสถานะจะดำเนินการเสร็จสิ้น

### ⚠️ ข้อกำหนดและข้อควรระวังในการใช้งาน

* **การใช้งานช่อง Custom Options:** มีไว้สำหรับระบุพารามิเตอร์เสริมของเอนจินดาวน์โหลด เช่น การระบุลำดับดาวน์โหลดแบบกลุ่มข้อมูล ให้ใช้คำสั่ง `--playlist-items 1-5`
* **ข้อจำกัดทางเทคนิค:** พึงละเว้นการกรอกข้อมูลใดๆ ลงในพื้นที่ใต้ส่วนแสดงผลตัวอักษรสีฟ้า เนื่องจากพื้นที่ดังกล่าวเป็นส่วนที่ระบบสงวนไว้สำหรับการปรับปรุงและแสดงสถานะภายในของแอปพลิเคชันเท่านั้น

### 💻 ขั้นตอนการคอมไพล์ระบบ (Build จากซอร์สโค้ด)

สำหรับผู้พัฒนาที่ต้องการนำซอร์สโค้ดไปพัฒนาต่อยอดหรือทำการคอมไพล์ระบบด้วยตนเอง:

1. เปิดไดเรกทอรีโครงการนี้ด้วยโปรแกรม **Microsoft Visual Studio**
2. เปิดไฟล์โซลูชันหลักที่มีนามสกุล `.sln`
3. ปรับเปลี่ยนรูปแบบการคอมไพล์ตรงแถบเครื่องมือด้านบนให้เป็นโปรไฟล์ `Release`
4. ดำเนินการสั่ง Build ระบบ ไฟล์ผลลัพธ์รูปแบบ `.exe` จะถูกสร้างขึ้นในไดเรกทอรี `bin\Release` พร้อมสำหรับการใช้งาน

---

## 🤝 Acknowledgements / กิตติกรรมประกาศ

This project relies on powerful open-source utilities. Sincere gratitude to the developers and communities behind:  
โปรเจกต์นี้ขับเคลื่อนด้วยเครื่องมือโอเพนซอร์สประสิทธิภาพสูง ขอกราบขอบพระคุณผู้พัฒนาและชุมชนผู้สร้างสรรค์เครื่องมือหลักมา ณ ที่นี้:

* **[yt-dlp](https://github.com/yt-dlp/yt-dlp)** — Advanced command-line media downloader utility.
* **[FFmpeg](https://ffmpeg.org/)** — Cross-platform solution to record, convert, and stream audio and video.

---

## ⚖️ Disclaimer / ข้อปฏิเสธความรับผิดชอบ

> This application is developed strictly for **educational purposes** and **personal use** only. The developer does not condone, support, or encourage any form of copyright infringement.
>  
> By using this software, you agree to the following terms:
> * **No Commercial Use:** You must not use this tool to download copyrighted material for commercial profit or unauthorized distribution.
> * **Respect Content Creators:** You are solely responsible for compliance with the terms of service of the respective hosting platforms and local copyright laws.
> * **User Liability:** The developer shall not be held liable or responsible for any misuse, copyright claims, or damages resulting from the use of this application.
>  
> แอปพลิเคชันนี้พัฒนาขึ้นเพื่อ **วัตถุประสงค์ทางการศึกษา** และ **การใช้งานส่วนบุคคล** เท่านั้น ผู้พัฒนาไม่สนับสนุนการละเมิดลิขสิทธิ์ทุกรูปแบบ ผู้ใช้งานต้องเป็นผู้รับผิดชอบต่อกฎหมายลิขสิทธิ์และเงื่อนไขของแพลตฟอร์มปลายทางด้วยตนเอง หากเกิดการนำไปใช้ในทางที่ผิดกฎหมาย ทางผู้พัฒนาจะไม่รับผิดชอบใด ๆ ทั้งสิ้น

---

## 📜 License / สัญญาอนุญาต

This project is licensed under the **GNU General Public License v3.0 (GPL-3.0)**:

> 🟩 **Permissions / สิทธิ์ในการใช้งาน** > Commercial use, modification, and distribution are permitted.  
> อนุญาตให้ผู้ที่สนใจนำซอร์สโค้ดนี้ไปใช้งาน ดัดแปลง แก้ไข หรือนำไปใช้ประโยชน์ในเชิงพาณิชย์เพื่อสร้างรายได้ได้อย่างถูกต้องตามกฎหมาย

> 🟧 **Conditions / เงื่อนไขผูกพัน (Copyleft)** > Any derivative works distributed at scale **must disclose their source code** under the same GPL-3.0 license, and include proper attribution to the original author.  
> หากมีการนำซอร์สโค้ดนี้ไปดัดแปลง แก้ไข หรือแจกจ่ายต่อในวงกว้าง ผู้ดำเนินการจำเป็นต้องทำการเปิดเผยซอร์สโค้ด (Disclose Source) ภายใต้สัญญาอนุญาตฉบับเดิม และต้องคงไว้ซึ่งประกาศสิทธิ์และเครดิตของผู้พัฒนาต้นฉบับ ห้ามลบออกโดยเด็ดขาด
