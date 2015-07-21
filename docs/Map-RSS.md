Map\RSS File Format
===================

```
Redmoon server file: Data\Map\Shop00*.rss
```

This file sets the shop inventories (`Data\Shop\Shop00*.rss`) that can be
opened on a particular map.

Take the following example of `Data\Shop\Shop00003.rss`:
```
13
14
```

The "3" in the filename references the map Downtown1. Shops 13 and 14 are 
enabled, which can be found at `Data\Shop\Shop00013.rss` and 
`Data\Shop\Shop00014.rss`, which are the Scientist and Watchman shops.

