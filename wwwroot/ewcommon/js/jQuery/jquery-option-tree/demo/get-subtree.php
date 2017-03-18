<?php
require_once 'LazyTaxonomyReader.php';

/**
 * Simple file outputing a given subtree in JSON
 *
 * This class is a part of:
 *
 * jQuery optionTree Plugin
 * version: 1.1
 * @requires jQuery v1.3 or later
 *
 * Dual licensed under the MIT and GPL licenses:
 *   http://www.opensource.org/licenses/mit-license.php
 *   http://www.gnu.org/licenses/gpl.html
 *
 * @version $Id: get-subtree.php 5 2010-09-23 17:26:23Z kkotowicz@gmail.com $
 * @author  Krzysztof Kotowicz <kkotowicz at gmail dot com>
 */
$reader = new LazyTaxonomyReader('taxonomy.txt');

$line_no = (isset($_GET['id']) && is_numeric($_GET['id']) ? (int) $_GET['id'] : null);
echo json_encode($reader->getDirectDescendants($line_no));
