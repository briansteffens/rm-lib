RME File Format
===============

```
Redmoon server file: Data\Extra\Extra*.rme
From Redmoon version: 3.8
Encoding: ASCII
```

Each of these files defines the interactable objects/NPCs to be placed on the
map specified by the 5-digit map index in the filename (ex: Extra00137.rme
controls NPCs on Uni).

Each line in the file looks about like the following example, which puts a bank
vendor in the Street2 bank (Extra00025.rme):

```
;comment  skin_id event    left    top    right  bottom random_move
Banker      21      6       8       8       10      9       0
```

The fields are:

| Field name      | Description                                         |
|-----------------|-----------------------------------------------------|
| comment         | Free-form tag/comment field                         |
| skin_id         | The character skin index (13=doctor, 19=vendor)     |
| event           | What happens when the object is clicked (see below) |
| left            | The left-most bound of the spawn rectangle          |
| top             | The top-most bound of the spawn rectangle           |
| right           | The right-most bound of the spawn rectangle         |
| bottom          | The bottom-most bound of the spawn rectangle        |
| random_move     | 0 for stationary, 1 for random movement             |

Event IDs reference entries in [Data\Event\EventInfo.rse](/docs/EventInfo.md).

Here is a partial list of events:

| Event ID | Description              |
|----------|--------------------------|
| 0        | Nothing                  |
| 6        | Bank clerk (access bank) |
| 8        | Doctor/nurse (full heal) |
| 25       | Army hall manager        |
| 33       | Sahara1 unique trader    |
| 36       | Kasham                   |
| 43       | Prison warden            |
| 80       | Signus bank              |
| 87       | Sunset upgrader          |
