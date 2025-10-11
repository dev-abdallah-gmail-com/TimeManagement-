#!/bin/bash

# This script demonstrates the complete workflow of the TimeManagement API

BASE_URL="http://localhost:5154"

echo "=== TimeManagement API Test Script ==="
echo ""

# 1. Register users
echo "1. Registering users..."
JOHN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "Password123!",
    "firstName": "John",
    "lastName": "Doe"
  }')

JOHN_TOKEN=$(echo $JOHN_RESPONSE | python3 -c "import sys, json; print(json.load(sys.stdin)['token'])")
echo "   John registered successfully"

JANE_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "jane.smith@example.com",
    "password": "Password123!",
    "firstName": "Jane",
    "lastName": "Smith"
  }')

JANE_TOKEN=$(echo $JANE_RESPONSE | python3 -c "import sys, json; print(json.load(sys.stdin)['token'])")
echo "   Jane registered successfully"
echo ""

# 2. John creates a task
echo "2. John creates a task..."
TASK_RESPONSE=$(curl -s -X POST "$BASE_URL/api/tasks" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $JOHN_TOKEN" \
  -d '{
    "title": "Complete Project Documentation",
    "description": "Write comprehensive documentation for the project",
    "scheduledStartDate": "2025-10-15T09:00:00Z",
    "scheduledEndDate": "2025-10-20T17:00:00Z"
  }')

TASK_ID=$(echo $TASK_RESPONSE | python3 -c "import sys, json; print(json.load(sys.stdin)['id'])")
echo "   Task created with ID: $TASK_ID"
echo ""

# 3. John assigns the task to Jane
echo "3. John assigns the task to Jane..."
curl -s -X POST "$BASE_URL/api/tasks/$TASK_ID/assign" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $JOHN_TOKEN" \
  -d '{
    "assigneeEmail": "jane.smith@example.com"
  }' > /dev/null
echo "   Task assigned to Jane"
echo ""

# 4. Jane accepts the task
echo "4. Jane accepts the task..."
curl -s -X POST "$BASE_URL/api/tasks/$TASK_ID/accept-reject" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $JANE_TOKEN" \
  -d '{
    "accept": true
  }' > /dev/null
echo "   Task accepted by Jane"
echo ""

# 5. Jane updates task status to InProgress
echo "5. Jane updates task status to InProgress..."
curl -s -X PATCH "$BASE_URL/api/tasks/$TASK_ID/status" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $JANE_TOKEN" \
  -d '3' > /dev/null
echo "   Task status updated to InProgress"
echo ""

# 6. Jane modifies the task details
echo "6. Jane modifies the task details..."
curl -s -X PUT "$BASE_URL/api/tasks/$TASK_ID" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $JANE_TOKEN" \
  -d '{
    "title": "Complete Project Documentation - Updated",
    "description": "Write comprehensive documentation for the project - Added more details",
    "scheduledStartDate": "2025-10-15T09:00:00Z",
    "scheduledEndDate": "2025-10-22T17:00:00Z"
  }' > /dev/null
echo "   Task details updated"
echo ""

# 7. Jane completes the task
echo "7. Jane completes the task..."
curl -s -X PATCH "$BASE_URL/api/tasks/$TASK_ID/status" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $JANE_TOKEN" \
  -d '4' > /dev/null
echo "   Task marked as completed"
echo ""

# 8. View John's tasks
echo "8. Viewing John's created tasks..."
curl -s -X GET "$BASE_URL/api/tasks/my-tasks" \
  -H "Authorization: Bearer $JOHN_TOKEN" | python3 -m json.tool
echo ""

# 9. View Jane's assigned tasks
echo "9. Viewing Jane's assigned tasks..."
curl -s -X GET "$BASE_URL/api/tasks/assigned-to-me" \
  -H "Authorization: Bearer $JANE_TOKEN" | python3 -m json.tool
echo ""

echo "=== Test completed successfully! ==="
