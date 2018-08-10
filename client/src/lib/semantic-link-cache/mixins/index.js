import _ from 'underscore';

import {AsyncCollectionMixins} from './asyncCollection';
import {CollectionMixins} from './collection';
import {RepresentationMixins} from './representation';
import {uriListMixins} from './uri-list';
import {uriMixins} from './uri';
import {linkRelMixins} from './linkRel';

_.mixin(AsyncCollectionMixins);
_.mixin(CollectionMixins);
_.mixin(RepresentationMixins);
_.mixin(uriListMixins);
_.mixin(uriMixins);
_.mixin(linkRelMixins);

export default _;