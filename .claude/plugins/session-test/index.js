const { Tool } = require("@opencode-ai/tool"); // 或者对应你版本的 Tool 导入

// 定义一个简单命令，保证插件被识别
module.exports = {
  sessionTestTool: Tool.define("session-test-command", async () => ({
    description: "A test command to make plugin recognized",
    parameters: {},
    execute: async () => {
      console.log("Plugin main file executed!");
      return "Plugin is loaded";
    }
  }))
};
