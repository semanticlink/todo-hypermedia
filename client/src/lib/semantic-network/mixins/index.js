import _ from 'underscore';

import {AsyncCollectionMixins} from './asyncCollection';
import {CollectionMixins} from './collection';
import {uriListMixins} from './uri-list';
import {uriMixins} from './uri';

_.mixin(AsyncCollectionMixins);
_.mixin(CollectionMixins);
_.mixin(uriListMixins);
_.mixin(uriMixins);

export default _;