# IntlTelInput - ExtJs Wrapper for International Telephone Input

[![authors](https://img.shields.io/badge/authors-scott%20meesseman-6F02B5.svg?logo=visual%20studio%20code)](https://www.perryjohnson.com)
[![app-type](https://img.shields.io/badge/category-extjs%20web-blue.svg)](https://www.perryjohnson.com)
[![app-lang](https://img.shields.io/badge/language-c%23%20javascript-blue.svg)](https://www.perryjohnson.com)
[![app-publisher](https://img.shields.io/badge/%20%20%F0%9F%93%A6%F0%9F%9A%80-app--publisher-e10000.svg)](https://github.com/perryjohnsoninc/app-publisher)

- [IntlTelInput - ExtJs Wrapper for International Telephone Input](#intltelinput---extjs-wrapper-for-international-telephone-input)
  - [Description](#description)
  - [Installation](#installation)
  - [Setup](#setup)
  - [Usage](#usage)

## Description

This package provides a full stack between an ExtJs client and a backend database using the ExtJS data package API.

## Installation

To install this module run the following command from the directory containing the project's package.json file:

    $ npm install @spmeesseman/extjs-pkg-intltelinput

## Setup

To include the package in an ExtJS application build, be sure to add the package name to the list of required packages in the app.json file:

    "requires": [
         "intltelinput",
        ...
    ]

For an open tooling build, also add the node_modules path to the workspace.json packages path array.

     "packages": {
        "dir": "...${workspace.dir}/node_modules/@spmeesseman/extjs-pkg-intltelinput"
    }

## Usage

An example of usage:

    require: [ 'Ext.ux.form.field.IntlPhone' ],
    items: [
    {
        xtype: 'intlphonefield'
    }]
