import _ from 'underscore';

import { AsyncCollectionMixins } from './asyncCollection';
import { CollectionMixins } from './collection';
import { RepresentationMixins } from './representation';

_.mixin(AsyncCollectionMixins);
_.mixin(CollectionMixins);
_.mixin(RepresentationMixins);

export default _;