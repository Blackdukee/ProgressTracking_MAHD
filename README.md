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


📑 Endpoints Overview
📘 1. Get Progress Summary
Description:
Retrieves a summary of a user’s learning progress, including enrolled courses, completed courses, videos watched, and total watch time.

📍 Request
Method: GET

URL: /api/v1/Progress/summary/{userId}

Headers:

Authorization: Bearer <JWT_TOKEN> (required)

Accept: application/json

Path Parameters:

userId (string, required) — e.g., test-user-123

📦 Example Request
http
نسخ
تحرير
GET http://localhost:5004/api/v1/Progress/summary/test-user-123
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Accept: application/json
📤 Response
Status: 200 OK

Content-Type: application/json

Body Example:

json
نسخ
تحرير
{
  "UserId": "test-user-123",
  "TotalCoursesEnrolled": 0,
  "CompletedCourses": 0,
  "TotalVideosWatched": 0,
  "TotalWatchTimeHours": 0,
  "RecentCourses": []
}
Response Schema:

json
نسخ
تحرير
{
  "UserId": "string",
  "TotalCoursesEnrolled": "integer",
  "CompletedCourses": "integer",
  "TotalVideosWatched": "integer",
  "TotalWatchTimeHours": "number",
  "RecentCourses": [
    {
      "Id": "string",
      "CourseId": "string",
      "CourseTitle": "string",
      "CompletedVideos": "integer",
      "TotalVideos": "integer",
      "CompletionPercentage": "number",
      "TotalWatchTimeSeconds": "number",
      "LastAccessed": "string (ISO 8601)"
    }
  ]
}
⚙️ Business Logic
Validate JWT token and role (Student)

Check cache (key: ProgressSummary_{userId})

If not cached:

Fetch enrollments (Enrollment API or mock)

Query database for progresses

Fetch course titles (CMS API or mock)

Aggregate metrics

Cache result (5 minutes)

Log via IAuditLogService

Return response

🔄 Data Transformations
Aggregate CourseProgresses & VideoProgresses

Map CMS course titles

Convert seconds → hours

⚠️ Error Handling
Status	Description	Response Body
200	Success	Progress summary
401	Unauthorized (invalid/missing token)	{ "title": "Unauthorized", "status": 401 }
403	Forbidden (not a Student)	{ "message": "Only Students can view progress" }
500	Internal Server Error	{ "message": "Failed to fetch progress summary" }

Notes:

Use mock data if UMS/Enrollment API unavailable (log warning)

📘 2. Enrollment Webhook
Description:
Processes enrollment updates (enroll/unenroll) triggered by the Enrollment API.

📍 Request
Method: POST

URL: /api/v1/progress/Webhook/enrollment-updated

Headers:

Authorization: Bearer <JWT_TOKEN> (required)

X-Server-Key: <SERVER_KEY> (required)

Content-Type: application/json

Accept: */*

Body:

json
نسخ
تحرير
{
  "UserId": "string",
  "CourseId": "string",
  "Action": "string" // "enroll" or "unenroll"
}
📦 Example Request
http
نسخ
تحرير
POST http://localhost:5004/api/v1/progress/Webhook/enrollment-updated
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
X-Server-Key: a1b2c3d4-e5f6-7890-abcd-ef1234567890
Content-Type: application/json
Accept: */*

Body:
{
  "UserId": "test-user-123",
  "CourseId": "course-7890",
  "Action": "enroll"
}
📤 Response
Status: 200 OK

Body: {} (empty object)

⚙️ Business Logic
Validate X-Server-Key (from appsettings.json)

Validate JWT token

Validate request body (UserId, CourseId, Action)

Fetch user role from UMS (or mock if fails)

Perform based on Action:

enroll:

Insert/update Enrollments table

Initialize CourseProgress record

unenroll:

Update Enrollments status or delete records

Invalidate cache (ProgressSummary_{userId})

Fetch course details (CMS API or mock)

Log action via IAuditLogService

Return 200 OK


