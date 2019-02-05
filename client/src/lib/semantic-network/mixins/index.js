import _ from 'underscore';

import {uriListMixins} from './uri-list';
import {uriMixins} from './uri';

_.mixin(uriListMixins);
_.mixin(uriMixins);

export default _;