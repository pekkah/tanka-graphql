# GraphQL @defer Directive Sample

This sample demonstrates the usage of the `@defer` directive for incremental delivery in GraphQL. It simulates a user profile system where basic information loads quickly while expensive fields (profile details, statistics, and activity) can be deferred.

## Running the Sample

```bash
dotnet run
```

Then navigate to http://localhost:5240/graphql/ui to access GraphiQL.

## Schema Overview

The sample implements a user profile system with:
- **Fast fields**: Basic user info (id, username, displayName, avatarUrl)
- **Slow fields**: Detailed profile, statistics, and recent activity

## Example Queries

### 1. Basic Query (No Defer)
This query fetches everything at once. Notice the delay before any data appears:

```graphql
query GetUserProfile {
  me {
    id
    username
    displayName
    avatarUrl
    profile {
      bio
      location
      website
      joinedAt
      followers
      following
    }
    stats {
      totalPosts
      totalLikes
      totalComments
      engagementRate
      topTags
    }
    recentActivity {
      id
      type
      description
      timestamp
    }
  }
}
```

### 2. Using @defer for Better UX
This query returns basic info immediately, then streams the expensive fields:

```graphql
query GetUserProfileWithDefer {
  me {
    id
    username
    displayName
    avatarUrl
    
    # Defer the profile data with a label
    ... @defer(label: "userProfile") {
      profile {
        bio
        location
        website
        joinedAt
        followers
        following
      }
    }
    
    # Defer statistics calculation
    ... @defer(label: "userStats") {
      stats {
        totalPosts
        totalLikes
        totalComments
        engagementRate
        topTags
      }
    }
    
    # Defer activity loading
    ... @defer(label: "recentActivity") {
      recentActivity {
        id
        type
        description
        timestamp
      }
    }
  }
}
```

### 3. Conditional Defer with Variables
Control whether to defer using variables:

```graphql
query GetUserProfileConditional($shouldDefer: Boolean!) {
  me {
    id
    username
    displayName
    
    ... @defer(if: $shouldDefer, label: "profileData") {
      profile {
        bio
        location
        followers
        following
      }
      stats {
        totalPosts
        engagementRate
      }
    }
  }
}
```

Variables:
```json
{
  "shouldDefer": true
}
```

### 4. Nested Defer
You can defer fragments within deferred fragments:

```graphql
query NestedDefer {
  me {
    id
    username
    
    ... @defer(label: "level1") {
      profile {
        bio
        location
      }
      
      ... @defer(label: "level2") {
        stats {
          totalPosts
          totalLikes
          engagementRate
        }
      }
    }
  }
}
```

## Response Format

When using @defer, the response arrives in multiple payloads:

### Initial Response
```json
{
  "data": {
    "me": {
      "id": "user-123",
      "username": "johndoe",
      "displayName": "John Doe",
      "avatarUrl": "https://example.com/avatars/johndoe.jpg"
    }
  },
  "hasNext": true
}
```

### Incremental Payloads
```json
{
  "incremental": [
    {
      "label": "userProfile",
      "path": ["me"],
      "data": {
        "profile": {
          "bio": "Software developer passionate about GraphQL and distributed systems.",
          "location": "San Francisco, CA",
          "website": "https://johndoe.dev",
          "joinedAt": "2022-01-15",
          "followers": 1234,
          "following": 567
        }
      }
    }
  ],
  "hasNext": true
}
```

### Final Payload
```json
{
  "incremental": [
    {
      "label": "recentActivity",
      "path": ["me"],
      "data": {
        "recentActivity": [
          {
            "id": "act-1",
            "type": "post",
            "description": "Published article about GraphQL @defer",
            "timestamp": "2025-01-15 10:30:00"
          }
        ]
      }
    }
  ],
  "hasNext": false
}
```

## Performance Benefits

1. **Improved Time to First Byte (TTFB)**: Users see basic info immediately
2. **Progressive Enhancement**: UI can render incrementally as data arrives
3. **Better Perceived Performance**: Critical data loads first
4. **Efficient Resource Usage**: Expensive operations don't block simple ones

## Client Implementation Tips

- Use the `hasNext` field to know when streaming is complete
- Handle incremental payloads by merging them into your client state
- Use labels to identify which parts of the UI to update
- Consider showing loading indicators for deferred sections

## Testing the Sample

1. **Start the application**: `dotnet run`
2. **Open GraphiQL**: Navigate to http://localhost:5240/graphql/ui
3. **Run the example queries** above to see incremental delivery in action
4. **Observe the response timing**:
   - Without @defer: All data arrives after ~4.5 seconds (1.5s + 2s + 1s delays)
   - With @defer: Basic info arrives immediately, then incremental payloads stream in
5. **Use the Network tab** in browser dev tools to see the streaming response

## What Makes This Real-World

This sample simulates realistic scenarios:
- **Basic user info** (fast database primary key lookup)
- **Profile data** (additional database joins and processing)
- **Statistics** (expensive aggregation queries across multiple tables)  
- **Recent activity** (time-ordered queries with pagination)

The artificial delays represent real network latency, database query time, and computational overhead you'd see in production systems.