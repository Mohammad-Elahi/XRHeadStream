# [XR Head Stream](https://github.com/Mohammad-Elahi/XRHeadStreamApp/)

XRHeadStreamApp is an advanced real-time head-tracking solution that streams head-tracking data from Meta Quest headsets to the Unity server.  Developed using Android NDK C++ and [Meta OpenXR SDK](https://developers.meta.com/horizon/downloads/package/oculus-openxr-mobile-sdk/), this application enables seamless wireless transmission of head-tracking data while maintaining Passthrough visualization for environmental awareness. This professional research tool was developed by Mohammad Elahi at [the Vodafone Chair for Mobile Communications Systems, TU Dresden.](https://www.vodafone-chair.org/) 

## Overview

This application serves as a bridge between Meta Quest headsets and Unity environments, providing real-time head tracking data streaming capabilities. The app utilizes the Quest's built-in tracking systems and Passthrough technology to deliver precise head motion data while allowing users to maintain awareness of their physical surroundings.

## Features

**Core Capabilities**
- Real-time wireless head tracking data streaming
- Unity integration for live motion visualization
- Passthrough support for environmental awareness
- Low-latency data transmission


**Technical Specifications**
- Built with Android NDK and C++
- Implements Meta OpenXR SDK
- Wireless communication protocol for Unity integration
- Real-time camera transformation updates

## Requirements

**Hardware**
- Meta Quest 2, Meta Quest Pro, or Meta Quest 3
- PC running Unity (for visualization)
- Wireless network connection

**Software**
- Meta OpenXR SDK 
- Unity 2022.3 or newer
- Meta Quest OS 


## Installation

1. Install the APK on your Meta Quest headset
2. Import the Unity package into your project
3. Configure the wireless connection settings
4. Start the Unity server
5. Launch the app on your Quest device

## Usage

1. Launch the Unity project
2. Start the XRHeadStreamApp on your Quest
3. Connect to the Unity server using your local network
4. Move your head to see real-time updates in Unity

## Development

This project is built using:
// Core technologies
- Android NDK
- Meta OpenXR SDK
- Unity Integration Package

## Author
Mohammad Elahi,
Research Assistant at Vodafone Chair for Mobile Communications Systems, TU Dresden, 
mohammad.elahi@mailbox.tu-dresden.de

## How to Cite
If you use this software in your research, please cite it as follows:
Elahi, M. (2024). XRHeadStream: Real-time head tracking streaming solution for Meta Quest headsets. GitHub. https://github.com/Mohammad-Elahi/XRHeadStreamApp

For BibTeX:
```cpp
@software{Elahi2024XRHeadStream,
    author = {Elahi, Mohammad},
    title = {XRHeadStreamApp: Real-time head tracking streaming solution for Meta Quest headsets},
    year = {2024},
    publisher = {GitHub},
    url = {https://github.com/Mohammad-Elahi/XRHeadStreamApp}, 
    institution = {Vodafone Chair for Mobile Communications Systems, TU Dresden}
}
```
