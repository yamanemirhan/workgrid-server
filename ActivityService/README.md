# Activity Service

ActivityService, NotificationService gibi ayr? bir mikroservis olarak çal??an ve Activity entity'lerini yöneten bir web API servisidir.

## Özellikler

- **Ayr? DbContext**: Sadece Activities tablosunu yönetir
- **RESTful API**: Activity'ler için CRUD operasyonlar?
- **RabbitMQ Integration**: Activity eventlerini dinler ve otomatik activity kayd? yapar
- **JWT Authentication**: Güvenli API eri?imi
- **Pagination**: Büyük veri setleri için sayfalama deste?i
- **Filtering**: Workspace, Board, Card, User bazl? filtreleme

## API Endpoints

### Activity Management
- `POST /api/activity` - Yeni activity olu?tur
- `GET /api/activity/{id}` - Specific activity'yi getir
- `DELETE /api/activity/{id}` - Activity'yi sil

### Activity Queries
- `GET /api/activity/workspace/{workspaceId}` - Workspace activity'leri
- `GET /api/activity/board/{boardId}` - Board activity'leri  
- `GET /api/activity/card/{cardId}` - Card activity'leri
- `GET /api/activity/user/{userId}` - User activity'leri
- `GET /api/activity/workspace/{workspaceId}/type/{type}` - Type bazl? activity'ler
- `GET /api/activity/workspace/{workspaceId}/recent` - Son activity'ler
- `GET /api/activity/workspace/{workspaceId}/count` - Activity say?s?

## Event Handling

ActivityService a?a??daki RabbitMQ event'lerini dinler ve otomatik activity kayd? yapar:

### Workspace Events
- `activity.workspace.created`
- `activity.workspace.updated` 
- `activity.workspace.deleted`

### Board Events
- `activity.board.created`
- `activity.board.updated`
- `activity.board.deleted`

### List Events
- `activity.list.created`
- `activity.list.updated`
- `activity.list.deleted`

### Card Events
- `activity.card.created`
- `activity.card.updated`
- `activity.card.deleted`
- `activity.card.moved`
- `activity.card.member.assigned`
- `activity.card.member.unassigned`

## Configuration

### Database
ActivityService kendi ActivityDbContext'ini kullan?r ve sadece Activities tablosunu yönetir.

### RabbitMQ
`appsettings.json` dosyas?nda RabbitMQ konfigürasyonu:

```json
{
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": "5672", 
    "UserName": "guest",
    "Password": "guest"
  }
}
```

### JWT
NotificationService ile ayn? JWT konfigürasyonunu kullan?r.

## Database Migration

?lk çal??t?rmadan önce migration olu?turun:

```bash
cd ActivityService
dotnet ef migrations add "ActivityDb Initial Migration" --context ActivityDbContext
```

Uygulama ba?lat?ld???nda migration otomatik olarak çal???r.

## Çal??t?rma

```bash
cd ActivityService
dotnet run
```

Service varsay?lan olarak `https://localhost:5001` portunda çal???r.

## Swagger UI

Development modunda Swagger UI aktif olur:
`https://localhost:5001/swagger`

## Örnek Kullan?m

### Manual Activity Olu?turma
```json
POST /api/activity
{
  "workspaceId": "guid",
  "type": "CardCreated",
  "description": "Yeni kart olu?turuldu",
  "boardId": "guid",
  "listId": "guid", 
  "cardId": "guid",
  "entityId": "guid",
  "entityType": "Card",
  "metadata": "{\"cardTitle\": \"Test Card\"}"
}
```

### Activity Sorgulama
```http
GET /api/activity/workspace/guid?page=1&pageSize=50
```

Service otomatik olarak RabbitMQ event'lerini dinler ve uygun activity kay?tlar? olu?turur.