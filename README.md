# FileCopyer / کپی کننده فایل

A powerful file copying utility with multi-threading support and progress monitoring capabilities.

یک ابزار قدرتمند برای کپی فایل با پشتیبانی از چند نخ و قابلیت نظارت بر پیشرفت.

## Features / ویژگی‌ها

- Multi-threaded file copying / کپی فایل چند نخی
- Progress monitoring with visual feedback / نظارت بر پیشرفت با بازخورد تصویری 
- Deep file verification / تأیید عمیق فایل
- Command-line interface support / پشتیبانی از رابط خط فرمان
- Configurable buffer size / اندازه بافر قابل تنظیم
- Parent path creation option / گزینه ایجاد مسیر والد
- Error logging and reporting / ثبت و گزارش خطا

## Usage / نحوه استفاده

FileCopyer.exe C:\\SourceFolder C:\\DestinationFolder --maxThreads 4

### GUI Mode / حالت رابط گرافیکی

1. Launch the application / برنامه را اجرا کنید
2. Configure settings / تنظیمات را پیکربندی کنید
3. Select source and destination / مبدا و مقصد را انتخاب کنید
4. Click "Copy" to start / برای شروع روی "کپی" کلیک کنید

### CLI Mode / حالت خط فرمان

FileCopyer.exe <source> <destination> [options]
Options / گزینه‌ها: 
--maxThreads <number>       Set maximum threads / تنظیم حداکثر تعداد نخ‌ها 
--createParentPath          Create parent path / ایجاد مسیر والد 
--showProgressBar          Show progress bar / نمایش نوار پیشرفت 
--checkFileDeep            Perform deep file check / انجام بررسی عمیق فایل 
--maxBufferSize <number>    Set buffer size (MB) / تنظیم اندازه بافر


## System Requirements / نیازمندی‌های سیستم

- .NET Framework 4.8.1
- Windows OS

## Building / ساخت

Open the solution in Visual Studio 2022 and build.

پروژه را در Visual Studio 2022 باز کرده و بسازید.

## License / مجوز

Copyright © 2024. All rights reserved.

حق کپی‌رایت محفوظ است © ۲۰۲۴
