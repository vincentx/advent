#!/usr/bin/env node

const {program} = require('commander');
const {spawn} = require('child_process');
const path = require('path');

program.version('1.0.0');

program
    .command('api')
    .description('Start semantic kenerl as Web API.')
    .action(() => run('kernel.api.dll', []))

program
    .command('index')
    .description('Index documents to semantic memory.')
    .argument('<folder>', 'Location of source documents')
    .option('-c, --collection <collection>', 'Collection name in semantic memory')
    .option('-i, --includes <extensions...>', 'File extensions to be indexed')
    .action((folder, options) => run('kernel.index.dll', [options.collection, folder, ...options.includes]))

function run(dll, args) {
    isDotnetCliInstalled().then((isInstalled) => {
        if (isInstalled) {
            const pathToApp = path.join(__dirname, 'bin', dll);
            const child = spawn('dotnet', [pathToApp, ...args]);
            child.stdout.pipe(process.stdout);
            child.stderr.pipe(process.stderr);
            child.on('exit', (code) => {
                process.exit(code);
            })
        } else {
            console.error('The .NET Core CLI (dotnet) is not installed. Please install it from https://dotnet.microsoft.com/download.');
            process.exit(1);
        }
    })
}

function isDotnetCliInstalled() {
    return new Promise((resolve) => {
        const child = spawn(process.platform === 'win32' ? 'where' : 'which', ['dotnet']);
        child.on('exit', (code) => {
            if (code === 0) {
                resolve(true);
            } else {
                resolve(false);
            }
        });
    });
}

program.parse(process.argv);