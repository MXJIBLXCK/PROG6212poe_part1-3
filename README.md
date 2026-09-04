# PROG6212poe_part1-3# RaceDay - Event Management System

**Course Code:** PROG6212  
**Portfolio of Evidence (POE)**

## Overview
RaceDay is a full-stack event management platform designed specifically for the South African road running, walking, and cycling community. It enables Event Organisers to publish and manage events and allows Participants to browse, register, and track performance history.

# Local Database Setup Guide

## Prerequisites
- Microsoft SQL Server 2019/2022 or LocalDB
- SQL Server Management Studio (SSMS) or Azure Data Studio

## Execution Steps
1. Launch SQL Server Management Studio and connect to your local server instance (`localhost` or `(localdb)\mssqllocaldb`).
2. Open `docs/RaceDay_Database.sql` inside SSMS.
3. Execute the script (`F5`).
4. Verify that `RaceDayDB` is generated under Object Explorer with all 6 populated tables.

## Project Structure
- `.github/workflows/` - CI/CD pipeline validation workflows.
- `docs/` - Contains system architecture plans, ERD diagrams, API endpoint specifications, and database scripts.

## System Roles
- **Organiser:** Manages events, categories, participant results, and views event enrolments.
- **Participant:** Browses events, registers for race categories, views enrolments, and tracks personal race history.

## CI/CD Status
![CI Build Status](../../workflows/Validate%20Documentation%20Folder/badge.svg)

## Video Presentation
*Link to unlisted YouTube presentation will be updated here.*
