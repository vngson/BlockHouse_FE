# BlockHouse

A barbershop management desktop application built with WPF and .NET 8.

## Features

- **Dashboard** - Revenue charts, employee rankings, and business metrics overview
- **Revenue Management** - Track revenue by date, employee, and service
- **Employee Management** - Add, edit, and manage barbershop staff
- **Service Management** - Manage services with pricing and descriptions

## Tech Stack

- **Framework**: WPF on .NET 8.0
- **Architecture**: MVVM (Model-View-ViewModel)
- **UI**: Material Design in XAML
- **Charts**: LiveCharts.Wpf
- **Backend API**: Runs locally on `http://localhost:5000`

## Project Structure

```
BlockHouse_FE/
├── Api/                    # API client classes
├── Assets/                 # Images and icons
├── Converters/             # XAML value converters
├── Helpers/                # Utility classes
├── Models/                 # Data models
├── Services/               # Backend process management
├── ViewModels/             # MVVM ViewModels
│   ├── Components/         # Reusable component VMs
│   └── Popup/              # Dialog VMs
├── Views/                  # XAML views
│   ├── Components/         # Reusable UI components
│   └── Popup/              # Dialog views
├── App.xaml                # App resources and theme
├── MainWindow.xaml         # Main window
└── BlockHouse.csproj       # Project configuration
```

## Prerequisites

- Visual Studio 2022 (v17.10+)
- .NET 8.0 SDK

## Getting Started

1. Clone the repository
2. Open `BlockHouse.sln` in Visual Studio
3. Restore NuGet packages
4. Build and run the solution

The application automatically starts the backend process (`app.exe`) on launch and connects to it via REST API.

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| CommunityToolkit.Mvvm | 8.4.0 | MVVM framework |
| MaterialDesignThemes | 5.2.1 | Material Design UI |
| MaterialDesignColors | 5.2.1 | Material Design colors |
| LiveCharts.Wpf | 0.9.7 | Data visualization charts |
| Newtonsoft.Json | 13.0.3 | JSON serialization |
| Haley.MVVM | 6.5.2 | MVVM utilities |
| System.Drawing.Common | 9.0.8 | Image handling |
