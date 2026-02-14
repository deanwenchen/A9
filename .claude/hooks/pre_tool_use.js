module.exports = async function preToolUse({ toolName, args }) {
    console.log("\x1b[33m%s\x1b[0m", `[PreToolUse] 即将执行工具: ${toolName}`);
    console.log("\x1b[33m%s\x1b[0m", '原始参数:', args);

    // 修改 my_tool 参数
    if (toolName === 'my_tool') {
        args.timestamp = Date.now();
        console.log("\x1b[33m%s\x1b[0m", '参数已修改:', args);
    }

    // 阻止执行 forbidden_tool
    if (toolName === 'forbidden_tool') {
        throw new Error('禁止执行 forbidden_tool');
    }

    return args;
};
