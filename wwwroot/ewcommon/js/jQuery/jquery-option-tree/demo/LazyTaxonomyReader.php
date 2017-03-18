<?php

/**
 * Class reading a given file (Google Base taxonomy file in this example)
 * and able to return direct descendands of a given line.
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
 * @version $Id: LazyTaxonomyReader.php 5 2010-09-23 17:26:23Z kkotowicz@gmail.com $
 * @author  Krzysztof Kotowicz <kkotowicz at gmail dot com>
 */
class LazyTaxonomyReader {

    private $base = null;
    private $separator = ' > ';
    protected $lines;

    public function __construct($file = 'taxonomy.txt') {
        $this->lines = file($file, FILE_IGNORE_NEW_LINES);
    }

    public function setBaseNode($line_no) {
        if (is_null($line_no)) {
            $this->base = null;
            return;
        }

        if (!array_key_exists($line_no, $this->lines)) {
            throw new IllegalArgumentException("Invalid line number.");
        }
        $this->base = $this->lines[$line_no];
    }

    public function getDirectDescendants($line_no = null) {
        $this->setBaseNode($line_no);
        // select only lines that are directly below current base node
        $direct = array_filter($this->lines, array($this, 'isDirectlyBelowBase'));
        // return only last part of their names
        return array_map(array($this, 'getLastNode'), $direct);
    }

    protected function getLastNode($line) {
        if (strpos($line, $this->separator) === false) {
            // no separator present
            return $line;
        }
        // strip up to and including last separator
        return substr($line, strrpos($line, $this->separator) + strlen($this->separator));
    }

    protected function isDirectlyBelowBase($line) {

        // starting text that must be present
        if (is_null($this->base)) {
            $start = '';
        } else {
            $start = $this->base . $this->separator;
        }

        if ($start !== '') {
            $starts_at_base = (strpos($line, $start) === 0);

            if (!$starts_at_base) { // starts with something different
                return false;
            }

            // remove start text AND the following separator
            $line = str_replace($start, '', $line);
        }

        // we're direct descendants if we have no separators left on the line
        if (strpos($line, $this->separator) !== false)
            return false;

        return true;
    }
}
