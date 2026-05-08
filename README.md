# [XR Head Stream](https://github.com/Mohammad-Elahi/XRHeadStreamApp/)

**XRHeadStream** is an advanced real-time head tracking pipeline that streams motion data from Meta Quest headsets, processes it through a standalone C++ predictor, and applies it to a Unity environment (OVRCameraRig) with ultra-low latency.

> **Note:** The Android client used to capture and stream the head-tracking data from the Meta Quest is an extended version of my previous project: [**XR-Head-Tracker-Pro**](https://github.com/Mohammad-Elahi/XR-Head-Tracker-Pro).

## Overview

This application serves as a bridge between Meta Quest headsets and Unity environments, providing real-time head tracking data streaming capabilities. The app utilizes the Quest's built-in tracking capabilities to send positional and rotational data over a local network, passing it through a custom middleware predictor for maximum responsiveness.

## Demo
https://github.com/user-attachments/assets/f1893a74-709e-443c-b330-9190a934ba36

## System Architecture

The project consists of three main components communicating via UDP on a local network:
<img width="1586" height="831" alt="XR Head Stream" src="https://github.com/user-attachments/assets/6b815f77-a17a-468a-b565-ca5ecd9541c0" />

1. **Client (Meta Quest / Android NDK App):** Streams raw pose data via UDP to Port `23001`.
2. **Predictor (C++ Middleware):** Receives the raw data, anticipates future movements, and waits for Unity's request on Port `23002`.
3. **Unity (Environment/Client):** Requests the pose and receives the ultra-fast predicted response on Port `23004` to update the `OVRCameraRig` position and rotation.

## Key Features
* **Real-Time Data Streaming:** Captures raw positional and rotational data from Meta Quest headsets using Android NDK and [Meta OpenXR SDK](https://developer.oculus.com/documentation/native/android/mobile-openxr/).
* **Smart Motion Predictor (C++):** A multi-threaded middleware application that uses historical pose data (Linear Separation) to anticipate future head movements. By calculating the *next pose* before the actual data arrives from the headset, it feeds Unity with ultra-fast, real-time tracking data.
* **Decoupled Architecture:** The headset and Unity client run independently at their own framerates. Instead of waiting for the headset's next network packet, Unity instantly queries the Predictor for the *future predicted pose* at any given timestamp, ensuring a much faster and smoother camera response locally.
* **UDP Socket Communication:** High-speed, low-overhead networking using Windows Sockets (`Winsock2`).
* **Passthrough Support:** Maintains environmental awareness while streaming data.

## Technical Specifications
- Built with Android NDK and C++
- Implements Meta OpenXR SDK
- Custom UDP wireless communication protocol
- Real-time camera transformation updates

## Requirements

**Hardware**
- Meta Quest 2, Meta Quest Pro, or Meta Quest 3
- PC running Unity (for visualization and running the Predictor)
- Local Wireless network connection (Headset and PC must be on the same network)

**Software**
- C++ Compiler (MSVC / GCC) and CMake (for building the Predictor)
- Meta OpenXR SDK 
- Unity 2022.3 or newer
- Meta Quest OS 

## Installation & Usage

Because the system is decoupled, you need to run the components in the following order:

**1. Start the C++ Predictor Middleware**
- Build the C++ Predictor project using CMake.
- Run the executable on your PC. It will automatically start listening in the background on your local IP.

**2. Start the Unity Environment**
- Open the Unity project containing the receiver script.
- Ensure your `OVRCameraRig` is configured properly.
- Press **Play** in the Unity Editor. Unity will start querying the local Predictor for pose data.

**3. Launch the App on Meta Quest**
- Download and install the pre-built APK (located in the `Android App` folder) on your Meta Quest headset using SideQuest or ADB.
- Connect the headset to the same local network as your PC.
- Open the app on the headset to begin streaming data to the PC.
- *Move your head to see real-time predicted updates in Unity!*

## Development
This project is built using:
- Android NDK
- Meta OpenXR SDK
- C++ (`Winsock2` and Multithreading)
- Unity Integration Package

## Author
**Mohammad Elahi**  
Research Assistant at Vodafone Chair for Mobile Communications Systems, TU Dresden  
mohammad.elahi@mailbox.tu-dresden.de

## How to Cite
If you use this software in your research, please cite it as follows:

Elahi, M. (2024). XRHeadStreamApp: Real-time head tracking streaming solution for Meta Quest headsets. GitHub. https://github.com/Mohammad-Elahi/XRHeadStreamApp

**BibTeX:**
```bibtex
@software{Elahi2024XRHeadStream,
    author = {Elahi, Mohammad},
    title = {XRHeadStreamApp: Real-time head tracking streaming solution for Meta Quest headsets},
    year = {2024},
    publisher = {GitHub},
    url = {https://github.com/Mohammad-Elahi/XRHeadStreamApp}, 
    institution = {Vodafone Chair for Mobile Communications Systems, TU Dresden}
}
```
