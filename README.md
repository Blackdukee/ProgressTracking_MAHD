# ProgressTracking_MAHD
![Screenshot 2025-06-15 005232](https://github.com/user-attachments/assets/558ae02f-14d9-4542-a895-1c043c2b9b85)
![Screenshot 2025-06-13 185931](https://github.com/user-attachments/assets/452e84b0-0519-4588-93fd-c7db84154e3d)
![Screenshot 2025-06-13 190303](https://github.com/user-attachments/assets/b20c58b0-87ea-4a5c-8350-0fc3f1a34714)
![Screenshot 2025-06-13 190451](https://github.com/user-attachments/assets/5cb78183-4142-4760-8407-a366f004d2b7)
![Screenshot 2025-06-13 220141](https://github.com/user-attachments/assets/18651fdd-f5a2-4ef0-b37a-15b0c1bdf3ad)

🗃️ Dependencies
Database: SQL Server

Tables: Enrollments, CourseProgresses, VideoProgresses

External APIs:

UMS API → http://localhost:5003

Enrollment API → http://localhost:5001

CMS API → http://localhost:5002
Sure — here’s a clean, clear, and properly formatted `README.md`-ready version of your **Progress Tracking System (PTS) API Endpoints Summary** in Markdown:

---

# 📊 Progress Tracking System (PTS) API — Endpoints Summary

## 📖 Overview

The **Progress Tracking System (PTS) API** is a RESTful service for tracking user progress in an educational platform. It supports functionalities like retrieving progress summaries, updating video/course progress, and handling enrollment updates via webhooks.

---

**Base URL:**
`http://localhost:5004/api/v1`

### 🔐 Authentication

* **JWT Token:** Required for most endpoints via header:
  `Authorization: Bearer <JWT_TOKEN>`

* **Server Key:** Required for webhook endpoints via header:
  `X-Server-Key: <SERVER_KEY>` (configured in `appsettings.json`)

---

## 📑 Endpoints Summary

| Method | Path                                   | Description                                                     | Authentication            |
| :----- | :------------------------------------- | :-------------------------------------------------------------- | :------------------------ |
| GET    | `/Progress/summary/{userId}`           | Retrieves a user’s progress summary.                            | JWT Token (Role: Student) |
| POST   | `/Progress/video`                      | Updates video progress and triggers course progress update.     | JWT Token (Role: Student) |
| GET    | `/Progress/course/{userId}/{courseId}` | Fetches detailed progress for a specific course.                | JWT Token (Role: Student) |
| POST   | `/progress/Webhook/enrollment-updated` | Processes enrollment updates from Enrollment API.               | JWT Token, X-Server-Key   |
| GET    | `/Progress/videos/{userId}/{courseId}` | Retrieves progress details for all videos in a specific course. | JWT Token (Role: Student) |

---

## 📌 Endpoint Details

### 1️⃣ GET `/Progress/summary/{userId}`

* **Purpose:** Provides a summary of a user’s learning progress.
* **Parameters:** `userId` (path, string, e.g., `test-user-123`)
* **Functionality:**

  * Validates JWT token role.
  * Checks cached summary (5 min TTL).
  * Aggregates data from database and external APIs.
  * Falls back to mock data if necessary.
* **Example:**
  `GET /api/v1/Progress/summary/test-user-123`
* **Response:** JSON with:

  * `TotalCoursesEnrolled`
  * `CompletedCourses`
  * `TotalVideosWatched`
  * `TotalWatchTimeHours`
  * `RecentCourses`

---

### 2️⃣ POST `/Progress/video`

* **Purpose:** Updates video progress for a user.
* **Request Body:**

```json
{
  "UserId": "test-user-123",
  "VideoId": "video-789",
  "CurrentTimeSeconds": 120,
  "MarkAsCompleted": false
}
```

* **Functionality:**

  * Validates JWT role.
  * Updates video progress and triggers course progress update.
  * Invalidates progress summary cache.
  * Logs activity.
* **Response:** JSON with updated video progress details.

---

### 3️⃣ GET `/Progress/course/{userId}/{courseId}`

* **Purpose:** Retrieves detailed progress for a specific course.
* **Parameters:** `userId`, `courseId` (path, strings)
* **Functionality:**

  * Validates JWT.
  * Queries course and video progress tables.
  * Fetches course title from CMS API (or mock data).
* **Example:**
  `GET /api/v1/Progress/course/test-user-123/course-7890`
* **Response:** JSON with:

  * Completion percentage
  * Completed videos
  * Total watch time

---

### 4️⃣ POST `/progress/Webhook/enrollment-updated`

* **Purpose:** Processes enrollment updates from the Enrollment API.
* **Request Body:**

```json
{
  "UserId": "test-user-123",
  "CourseId": "course-7890",
  "Action": "enroll"
}
```

* **Functionality:**

  * Validates `X-Server-Key` and JWT.
  * Updates enrollment and course progress records.
  * Invalidates cache.
  * Logs event.
* **Response:** `{}` (Empty JSON with 200 OK)
* **Note:** Requires valid `X-Server-Key` (e.g., `a1b2c3d4-e5f6-7890-abcd-ef1234567890`)

---

### 5️⃣ GET `/Progress/videos/{userId}/{courseId}`

* **Purpose:** Lists progress for all videos in a course.
* **Parameters:** `userId`, `courseId` (path, strings)
* **Functionality:**

  * Validates JWT.
  * Retrieves video progress data.
  * Fetches video titles from CMS API (or mock data).
* **Example:**
  `GET /api/v1/Progress/videos/test-user-123/course-7890`
* **Response:** JSON array of video progress details.

---

## 🔒 Authentication Notes

* **JWT Token:**
  Issued by UMS API, containing `sub` (UserId) and `role` claims. Must be valid and match expected audience.

* **X-Server-Key:**
  Configured in `appsettings.json`, required for webhook requests.

**Example Webhook Headers:**

```
Authorization: Bearer <JWT_TOKEN>
X-Server-Key: 
```

---

## 🚨 Error Codes

| Status Code               | Description            | Common Causes                       |
| :------------------------ | :--------------------- | :---------------------------------- |
| 200 OK                    | Success                | -                                   |
| 400 Bad Request           | Invalid input          | Missing/invalid parameters or body  |
| 401 Unauthorized          | Authentication failure | Invalid/missing JWT or X-Server-Key |
| 403 Forbidden             | Authorization failure  | User not a Student role             |
| 404 Not Found             | Resource not found     | Invalid UserId or CourseId          |
| 500 Internal Server Error | Server error           | Database or API failure             |


