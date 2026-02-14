const fs = require("fs");
const path = require("path");

// 获取项目根目录
const projectRoot = process.env.CLAUDE_PROJECT_ROOT || process.cwd();

// 生成测试文件路径
const filePath = path.join(projectRoot, "hook_test.txt");

// 写入触发时间
fs.writeFileSync(
  filePath,
  `SessionStart hook fired at ${new Date().toISOString()}\n`,
  { flag: "a" }
);

console.log("SessionStart hook executed, file created:", filePath);
